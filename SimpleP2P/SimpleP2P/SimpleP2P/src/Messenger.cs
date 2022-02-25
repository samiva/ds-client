using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleP2P
{
	internal enum MsgContent : byte {
		MSG_GREET    = 0x01,
		MSG_FAREWELL = 0x02,
		MSG_PING     = 0x03,
		MSG_DATA     = 0X04
	};

	internal enum MsgType : byte {
		MSG_TYPE_MANAGEMENT = 0x01,
		MSG_TYPE_DATA       = 0x02,
		MSG_TYPE_ACK_FLAG   = 0x80,
		MSG_TYPE_MASK       = 0x0f
	};

	/// <summary>
	/// Class for managing network traffic. Implements ack for each message and invokes
	/// events if messaging succeeds or fails.
	/// </summary>
	sealed internal class Messenger	
	{
		
		private const int  TIMEOUT      = 300000; // ms
		
		private          long                      msgId;
		readonly private Dictionary<long, Message> pendingMsgs;
		readonly private P2PManager                p2pman;
		readonly private Client                    client;
		readonly private CancellationTokenSource   token;
		private          Task?                     failTask;
		
		readonly private object pendingLock = new object ();
		
		public Messenger (P2PManager p2pman, Client client) {
			this.p2pman      = p2pman;
			this.client      = client;
			this.pendingMsgs = new Dictionary<long, Message> ();
			this.token       = new CancellationTokenSource ();
		}

		/// <summary>
		/// Processes raw byte stream obtained through a socket connection.
		/// </summary>
		/// <param name="peer">Sender IP and port</param>
		/// <param name="data">Raw data in byte array</param>
		public void handle (IPEndPoint peer, byte[] data) {
			RawMessage msg = new RawMessage (data);
			if (msg.isAck ()) {
				if (!this.receive (peer, msg)) {
					return;
				}
				switch (msg.msg) {
					case MsgContent.MSG_GREET:		this.acceptGreetAck (peer, msg.id);			break;
					case MsgContent.MSG_FAREWELL:	this.acceptFarewellAck (peer, msg.id);		break;
					case MsgContent.MSG_PING:		this.acceptPingAck (peer, msg.id);			break;
					case MsgContent.MSG_DATA:		this.acceptMsgAck (peer, msg.id, msg.data);	break;
				}
			} else {
				switch (msg.msg) {
					case MsgContent.MSG_GREET:		this.ackGreet (peer, msg.id);				break;
					case MsgContent.MSG_FAREWELL:	this.ackFarewell (peer, msg.id);			break;
					case MsgContent.MSG_PING:		this.ackPing (peer, msg.id);				break;
					case MsgContent.MSG_DATA:		this.ackMsg (peer, msg.id, msg.data);		break;	
				}
			}
		}
		
		public long greetPeer (IPEndPoint peer) {
			return this.sendManagement (peer, MsgContent.MSG_GREET);
		}

		public long farewellPeer (IPEndPoint peer) {
			return this.sendManagement (peer, MsgContent.MSG_FAREWELL);
		}

		public long pingPeer (IPEndPoint peer) {
			return this.sendManagement (peer, MsgContent.MSG_PING);
		}
		
		public long sendMsg (IPEndPoint peer, byte[] msg) {
			long id = this.getId ();
			return this.send (peer, new RawMessage (MsgType.MSG_TYPE_DATA, MsgContent.MSG_DATA, id, msg));
		}

#region Ack

		private void ackGreet (IPEndPoint peer, long id) {
			this.send (peer, new RawMessage (
			MsgType.MSG_TYPE_MANAGEMENT | MsgType.MSG_TYPE_ACK_FLAG,
				MsgContent.MSG_GREET,
				id
			));
			this.p2pman.OnPeerGreets (this, new ConnectionEventArgs (id, peer));
		}
		
		private void ackFarewell (IPEndPoint peer, long id) {
			this.send (peer, new RawMessage (
		   MsgType.MSG_TYPE_MANAGEMENT | MsgType.MSG_TYPE_ACK_FLAG,
				MsgContent.MSG_FAREWELL,
				id
			));
			this.p2pman.OnPeerFarewells (this, new ConnectionEventArgs (id, peer));
		}

		private void ackPing (IPEndPoint peer, long id) {
			this.send (peer, new RawMessage (
				MsgType.MSG_TYPE_MANAGEMENT | MsgType.MSG_TYPE_ACK_FLAG,
				MsgContent.MSG_PING,
				id
			));
		}
		
		private void ackMsg (IPEndPoint peer, long id, byte[] msg) {
			this.send (peer, new RawMessage (
		    MsgType.MSG_TYPE_DATA | MsgType.MSG_TYPE_ACK_FLAG,
				MsgContent.MSG_DATA,
				id
			));
			this.p2pman.OnMsg (this, new ConnectionEventArgs (id, peer, msg));
		}
		
#endregion
		
#region Accept ack		
		
		private void acceptGreetAck (IPEndPoint peer, long id) {
			this.p2pman.OnGreetSucceeded (this, new ConnectionEventArgs (id, peer));
		}
		
		private void acceptFarewellAck (IPEndPoint peer, long id) {
			this.p2pman.OnFarewellSucceeded (this, new ConnectionEventArgs (id, peer));
		}

		private void acceptPingAck (IPEndPoint peer, long id) {
			// this.p2pman.OnPingSucceeded or something...
		}
		
		private void acceptMsgAck (IPEndPoint peer, long id, byte[] msg) {
			this.p2pman.OnSendSucceeded (this, new ConnectionEventArgs (id, peer, msg));
		}

#endregion

#region Utility

		private long getId () {
			return Interlocked.Increment (ref this.msgId);
		}

		private long sendManagement (IPEndPoint peer, MsgContent msg) {
			long id = this.getId ();
			return this.send (peer, new RawMessage (MsgType.MSG_TYPE_MANAGEMENT, msg, id));
		}

		private long send (IPEndPoint peer, RawMessage msg) {
			lock (this.pendingLock) {
				this.pendingMsgs [msg.id] = new Message (peer, now (), msg);
			}
			this.client.send (peer, msg.make ());
			return msg.id;
		}

		private bool receive (IPEndPoint peer, RawMessage msg) {
			lock (this.pendingLock) {
				if (this.pendingMsgs.TryGetValue (msg.id, out Message? message) && message.peer.Equals (peer)) {
					this.pendingMsgs.Remove (msg.id);
					return true;
				}
				return false;
			}
		}
		
		static private long now () {
			return DateTimeOffset.Now.ToUnixTimeMilliseconds();
		}
		
#endregion

#region Failure watchdog		
		
		public void runFailTask () {
			this.failTask = Task.Run (
				() => { this.failPendingRoutine (TIMEOUT); },
				this.token.Token
			);
		}

		public void endFailTask () {
			this.token.Cancel ();
		}

		async private void failPendingRoutine (int timeout) {
			TimeSpan delay = TimeSpan.FromMilliseconds (1000);
			while (!this.token.IsCancellationRequested) {
				this.failPendingMessages (timeout);
				try {
					await Task.Delay (delay, this.token.Token);
				} catch (TaskCanceledException e) {
				}
			}
		}

		private void failPendingMessages (int timeout) {
			lock (this.pendingLock) {
				long       now  = Messenger.now ();
				List<long> dead = new List<long> ();
				foreach (KeyValuePair<long, Message> msg in this.pendingMsgs) {
					if (now - msg.Value.time > timeout) {
						dead.Add (msg.Key);
					}
				}
				foreach (long id in dead) {
					Message msg = this.pendingMsgs [id];
					this.pendingMsgs.Remove (id);
					switch (msg.msg.msg) {
						case MsgContent.MSG_GREET:		this.p2pman.OnGreetFailed (this, new ConnectionEventArgs (id, msg.peer));		break;
						case MsgContent.MSG_FAREWELL:	this.p2pman.OnFarewellFailed (this, new ConnectionEventArgs (id, msg.peer));	break;
						case MsgContent.MSG_PING:		break;
						case MsgContent.MSG_DATA:		this.p2pman.OnSendFailed (this, new ConnectionEventArgs (id, msg.peer, msg.msg.data));break;
					}
				}
			}
		}

#endregion
		
	}
}
