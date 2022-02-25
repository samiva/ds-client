using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using BombPeliLib.Events;
using BombPeliLib.Msgs;
using SimpleP2P;

namespace BombPeliLib
{
	readonly internal struct MsgEntry
	{
		readonly public long                        id;
		readonly public Action<ConnectionEventArgs> success;
		readonly public Action<ConnectionEventArgs> failure;

		public MsgEntry (long id, Action<ConnectionEventArgs> success, Action<ConnectionEventArgs> failure) {
			this.id      = id;
			this.success = success;
			this.failure = failure;
		}
	}

	/// <summary>
	/// Programming API class for the Simple P2P library. Implements game's messaging protocol
	/// and calls event handlers.
	/// </summary>
	public class P2PApi
	{

		public event EventHandler<PeerJoinedEventArgs>?       PeerJoined;
		public event EventHandler<PeerLeftEventArgs>?         PeerLeft;
		public event EventHandler<PeerListReceivedEventArgs>? PeerListReceived;
		public event EventHandler<GameStartEventArgs>?        GameStarts;
		public event EventHandler<BombReceivedEventArgs>?     BombReceived;
		public event EventHandler<PeerLostEventArgs>?         PeerLost;
		public event EventHandler<ConnectionEventArgs>?       PacketReceived;
		public event EventHandler<ConnectionEventArgs>?       PacketSent;

		// Events for own message send failures
		public event EventHandler<P2PCommEventArgs>? JoinFailed;
		public event EventHandler<P2PCommEventArgs>? LeaveFailed;
		public event EventHandler<P2PCommEventArgs>? RequestPeerListFailed;
		public event EventHandler<P2PCommEventArgs>? SendPeerListFailed;
		public event EventHandler<P2PCommEventArgs>? StartGameFailed;
		public event EventHandler<P2PCommEventArgs>? SendBombFailed;
		public event EventHandler<P2PCommEventArgs>? SendLoseFailed;
		public event EventHandler<P2PCommEventArgs>? PeerLeftFailed;
		public event EventHandler<P2PCommEventArgs>? PeerJoinedFailed;

		private          P2PManager                           p2pman;
		readonly private List<PeerInfo>                       peers;
		readonly private int                                  port;
		readonly private bool                                 isHost;
		readonly private ConcurrentDictionary<long, MsgEntry> msgs;
		readonly private object                               doLock             = new object ();
		private          long                                 packetSendCount    = 0;
		private          long                                 packetReceiveCount = 0;

		public int  Port   { get { return this.port; } }
		public bool IsHost { get { return this.isHost; } }

		public long getPacketSendCount { get { return this.packetSendCount; } }

		public long getPacketReceiveCount { get { return this.packetReceiveCount; } }

		readonly private object peersLock = new object ();

		public P2PApi (int port, bool isHost) {
			this.port   = port;
			this.isHost = isHost;
			this.peers  = new List<PeerInfo> ();
			this.msgs   = new ConcurrentDictionary<long, MsgEntry> ();
			this.p2pman = new P2PManager ((ushort)port);
		}

		public List<PeerInfo> Peers {
			get {
				List<PeerInfo> peers = new List<PeerInfo> (this.peers.Count);
				lock (this.peersLock) {
					foreach (PeerInfo peer in this.peers) {
						peers.Add (peer);
					}
				}
				return peers;
			}
		}

		public void Open () {
			this.p2pman.PeerGreets        += this.OnPeerGreets;
			this.p2pman.GreetSucceeded    += this.OnCommunicationSucceeded;
			this.p2pman.GreetFailed       += this.OnCommunicationFailed;
			this.p2pman.PeerFarewells     += this.OnPeerFarewells;
			this.p2pman.FarewellSucceeded += this.OnCommunicationSucceeded;
			this.p2pman.FarewellFailed    += this.OnCommunicationFailed;
			this.p2pman.MessageReceived   += this.OnMessageReceived;
			this.p2pman.SendSucceeded     += this.OnCommunicationSucceeded;
			this.p2pman.SendFailed        += this.OnCommunicationFailed;
			this.p2pman.SocketDied        += this.OnConnectionDied;
			this.p2pman.start ();
		}

		public void Close () {
			this.p2pman.end ();
			this.p2pman.PeerGreets        -= this.OnPeerGreets;
			this.p2pman.GreetSucceeded    -= this.OnCommunicationSucceeded;
			this.p2pman.GreetFailed       -= this.OnCommunicationFailed;
			this.p2pman.PeerFarewells     -= this.OnPeerFarewells;
			this.p2pman.FarewellSucceeded -= this.OnCommunicationSucceeded;
			this.p2pman.FarewellFailed    -= this.OnCommunicationFailed;
			this.p2pman.MessageReceived   -= this.OnMessageReceived;
			this.p2pman.SendSucceeded     -= this.OnCommunicationSucceeded;
			this.p2pman.SendFailed        -= this.OnCommunicationFailed;
			this.p2pman.SocketDied        -= this.OnConnectionDied;
		}
		
#region Do action

		public Task<bool> DoJoinGame (IPEndPoint peer) {
			TaskCompletionSource<bool> future = new TaskCompletionSource<bool> ();
			lock (this.doLock) {
				long id = this.greet (peer);
				this.msgs [id] = new MsgEntry (id, SendJoin, FailJoin);
			}
			return future.Task;

			void SendJoin (ConnectionEventArgs e) {
				Msg  data = new Msg (P2PConstants.JOIN);
				lock (this.doLock) {
					long id = this.send (data, e.peer);
					this.msgs [id] = new MsgEntry (id, AcknowledgeJoin, FailJoin);
				}
			}

			void AcknowledgeJoin (ConnectionEventArgs e) {
				this.AddPeer (e.peer);
				future.SetResult (true);
			}
			
			void FailJoin (ConnectionEventArgs e) {
				this.JoinFailed?.Invoke (this, new P2PCommEventArgs(e.peer, new JsonElement ()));
				future.SetResult (false);
			}
		}

		public Task<bool> DoLeaveGame (IPEndPoint peer) {
			TaskCompletionSource<bool> future = new TaskCompletionSource<bool> ();
			Msg  data = new Msg (P2PConstants.QUIT);
			lock (this.doLock) {
				long id = this.send (data, peer);
				this.msgs [id] = new MsgEntry (id, SendFarewell, FailLeave);
			}
			return future.Task;
			
			void SendFarewell (ConnectionEventArgs e) {
				lock (this.doLock) {
					long id = this.farewell (e.peer);
					this.msgs [id] = new MsgEntry (id, AcknowledgeLeave, FailLeave);
				}
			}

			void AcknowledgeLeave (ConnectionEventArgs e) {
				this.ClearPeers ();
				future.SetResult (true);
			}

			void FailLeave (ConnectionEventArgs e) {
				this.LeaveFailed?.Invoke (this, new P2PCommEventArgs(e.peer, new JsonElement ()));
				future.SetResult (false);
			}
		}
		
		public Task<bool> DoRequestPeerList (IPEndPoint peer) {
			TaskCompletionSource<bool> future = new TaskCompletionSource<bool> ();
			Msg                        data   = new Msg (P2PConstants.LIST_PEERS);
			lock (this.doLock) {
				long id = this.send (data, peer);
				this.msgs [id] = new MsgEntry (id, success, failure);
			}

			return future.Task;

			void success (ConnectionEventArgs e) {
				future.SetResult (true);
			}

			void failure (ConnectionEventArgs e) {
				this.RequestPeerListFailed?.Invoke (this, new P2PCommEventArgs(e.peer, new JsonElement ()));
				future.SetResult (false);
			}
		}
		
		public Task<bool> DoStartGame (IPEndPoint peer, int bombTime) {
			TaskCompletionSource<bool> future = new TaskCompletionSource<bool> ();
			Msg                        data   = new Msg (P2PConstants.START, objectToJson (new { bombtime = bombTime }));
			lock (this.doLock) {
				long id = this.send (data, peer);
				this.msgs [id] = new MsgEntry (id, success, failure);
			}
			return future.Task;

			void success (ConnectionEventArgs e) {
				future.SetResult (true);
			}
			
			void failure (ConnectionEventArgs e) {
				this.StartGameFailed?.Invoke (this, new P2PCommEventArgs(e.peer, new JsonElement ()));
				future.SetResult (false);
			}
		}
		
		public Task<bool> DoSendBomb (IPEndPoint peer, int bombTime) {
			TaskCompletionSource<bool> future = new TaskCompletionSource<bool> ();
			Msg data = new Msg (P2PConstants.PASS_BOMB, objectToJson (new {
				bombtime = bombTime,
				status   = GameStatus.RUNNING
			}));
			lock (this.doLock) {
				long id = this.send (data, peer);
				this.msgs [id] = new MsgEntry (id, success, failure);
			}

			return future.Task;

			void success (ConnectionEventArgs e) {
				future.SetResult (true);
			}

			void failure (ConnectionEventArgs e) {
				this.SendBombFailed?.Invoke (this, new P2PCommEventArgs(e.peer, new JsonElement ()));
				future.SetResult (false);
			}
		}

		private Task<bool> DoSendLose (IPEndPoint peer) {
			TaskCompletionSource<bool> future = new TaskCompletionSource<bool> ();
			
			Msg  data = new Msg (P2PConstants.LOSE, objectToJson( new { status = GameStatus.ENDED }));
			lock (this.doLock) {
				long id = this.send (data, peer);
				this.msgs [id] = new MsgEntry (id, success, failure);
			}
			return future.Task;

			void success (ConnectionEventArgs e) {
				Task<bool> result = this.DoLeaveGame (e.peer);
				result.Wait ();
				future.SetResult (result.Result);
			}

			void failure (ConnectionEventArgs e) {
				this.SendLoseFailed?.Invoke (this, new P2PCommEventArgs(e.peer, new JsonElement ()));
				future.SetResult (false);
			}
		}

		private Task<bool> DoSendPeerJoined (IPEndPoint peer, IPEndPoint joined) {
			TaskCompletionSource<bool> future = new TaskCompletionSource<bool> ();
			Msg                        data   = new Msg (P2PConstants.PEER_JOINED, objectToJson (new { peer = joined.ToString () }));
			lock (this.doLock) {
				long id = this.send (data, peer);
				this.msgs [id] = new MsgEntry (id, success, failure);
			}
			return future.Task;

			void success (ConnectionEventArgs e) {
				future.SetResult (true);
			}

			void failure (ConnectionEventArgs e) {
				this.PeerJoinedFailed?.Invoke (this, new P2PCommEventArgs(e.peer, new JsonElement ()));
				future.SetResult (false);
			}
		}

		private Task<bool> DoSendPeerLeft (IPEndPoint peer, IPEndPoint quit) {
			TaskCompletionSource<bool> future = new TaskCompletionSource<bool> ();
			
			Msg data = new Msg (P2PConstants.PEER_QUIT, objectToJson (new { peer = quit.ToString () }));
			lock (this.doLock) {
				long id = this.send (data, peer);
				this.msgs [id] = new MsgEntry (id, success, failure);
			}
			return future.Task;

			void success (ConnectionEventArgs e) {
				future.SetResult (true);
			}
			
			void failure (ConnectionEventArgs e) {
				this.PeerLeftFailed?.Invoke (this, new P2PCommEventArgs (e.peer, new JsonElement ()));
				future.SetResult (false);
			}
		}

		private Task<bool> DoSendPeerList (IPEndPoint peer) {
			TaskCompletionSource<bool> future = new TaskCompletionSource<bool> ();
			lock (this.peersLock) {
				string[] p = new string[this.peers.Count];
				int      i = 0;
				foreach (PeerInfo pi in this.peers) {
					if (!pi.ip.Equals (peer)) {
						p [i] = pi.ip.ToString ();
						++i;
					}
				}
				Array.Resize (ref p, i);
				Msg  data = new Msg (P2PConstants.PEERS, objectToJson (new { peerlist = p }));
				lock (this.doLock) {
					long id = this.send (data, peer);
					this.msgs [id] = new MsgEntry (id, success, failure);
				}
			}
			return future.Task;

			void success (ConnectionEventArgs e) {
				future.SetResult (true);
			}

			void failure (ConnectionEventArgs e) {
				this.SendPeerListFailed?.Invoke (this, new P2PCommEventArgs (e.peer, new JsonElement ()));
				future.SetResult (false);
			}
		}

#endregion

#region Broadcast
		
		public Task<bool[]> DoBroadcastLose () {
			lock (this.peersLock) {
				Task<bool>[] tasks = new Task<bool>[this.peers.Count];
				int          i     = 0;
				foreach (PeerInfo peer in this.peers) {
					tasks [i] = this.DoSendLose (peer.ip);
					++i;
				}
				return Task.WhenAll (tasks);
			}
		}

		public Task<bool[]> DoBroadcastPeerJoined (IPEndPoint joined) {
			PeerInfoComparer compare = PeerInfoComparer.instance;
			PeerInfo         tmp     = new PeerInfo (joined);
			lock (this.peersLock) {
				Task<bool>[] tasks = new Task<bool>[this.peers.Count];
				int          i     = 0;
				foreach (PeerInfo p in this.peers) {
					if (!compare.Equals (p, tmp)) {
						tasks [i] = this.DoSendPeerJoined (p.ip, joined);
						++i;
					}
				}
				Array.Resize (ref tasks, i);
				return Task.WhenAll (tasks);
			}
		}

		public Task<bool[]> BroadcastPeerQuit (IPEndPoint peer) {
			PeerInfoComparer compare = PeerInfoComparer.instance;
			PeerInfo         tmp     = new PeerInfo (peer);
			lock (this.peersLock) {
				Task<bool>[] tasks = new Task<bool>[this.peers.Count];
				int          i     = 0;
				foreach (PeerInfo p in this.peers) {
					if (!compare.Equals (p, tmp)) {
						tasks [i] = this.DoSendPeerLeft (p.ip, peer);
						++i;
					}
				}
				Array.Resize (ref tasks, i);
				return Task.WhenAll (tasks);
			}
		}

#endregion

#region Primitive actions
		
		private long greet (IPEndPoint peer) {
			long id = this.p2pman!.greet (peer);
			this.OnPacketSent(this, new ConnectionEventArgs(id, peer, null));
			return id;
		}

		private long farewell (IPEndPoint peer) {
			long id = this.p2pman!.farewell (peer);
			this.OnPacketSent(this, new ConnectionEventArgs(id, peer, null));
			return id;
		}

		private long send (Msg data, IPEndPoint peer) {
			long id = this.p2pman!.send (peer, encodeMsg (data));
			++this.packetSendCount;
			this.OnPacketSent(this, new ConnectionEventArgs(id, peer, null));
			return id;
		}
		
		private void OnCommunicationSucceeded (object? obj, ConnectionEventArgs e) {
			MsgEntry msg;
			lock (this.doLock) {
				if (!this.msgs.TryRemove (e.id, out msg)) {
					throw new Exception ("Impossible condition. Invalid response data.");
				}
			}
			msg.success.Invoke (e);
			this.OnPacketReceived(this, new ConnectionEventArgs(e.id, e.peer, e.data));
		}

		private void OnCommunicationFailed (object? obj, ConnectionEventArgs e) {
			MsgEntry msg;
			lock (this.doLock) {
				if (!this.msgs.TryRemove (e.id, out msg)) {
					Debug.WriteLine ("Impossible condition. Invalid response data.");
					return;
				}
			}
			msg.failure.Invoke (e);
			this.OnPacketReceived(this, new ConnectionEventArgs(e.id, e.peer, e.data));
		}

		private void OnPacketReceived(object? obj, ConnectionEventArgs e) {
			++this.packetReceiveCount;
			this.PacketReceived?.Invoke(obj, e);
		}

		private void OnPacketSent(object? obj, ConnectionEventArgs e) {
			this.PacketSent?.Invoke(obj, e);
		}

#endregion

#region Handlers

		private void OnPeerGreets (object? obj, ConnectionEventArgs e) {
			this.OnPacketReceived(obj, e);
		}

		private void OnPeerFarewells (object? obj, ConnectionEventArgs e) {
			this.OnPacketReceived(obj, e);
		}

		private void OnConnectionDied (object? obj, EventArgs e) {
			Debug.WriteLine ("asd");
		}

		private void OnMessageReceived (object? obj, ConnectionEventArgs e) {
			Msg?         msg;
			JsonElement? json = null;
			try {
				msg = decodeMsg (e.data);
			} catch {
				return;
			}
			if (msg == null) {
				return;
			}
			if (msg.data == null) {
			} else if (msg.data is JsonElement tmp && tmp.ValueKind == JsonValueKind.Object) {
				json = tmp;
			} else {
				return;
			}
			P2PCommEventArgs ea = new P2PCommEventArgs (e.peer, json ?? objectToJson (new {}));
			switch (msg.msg) {
				case P2PConstants.PASS_BOMB:   this.HandleBombReceived (ea);		break;
				case P2PConstants.LOSE:        this.HandlePeerLostReceived (ea);	break;
				case P2PConstants.JOIN:        this.HandleJoinReceived (ea);		break;
				case P2PConstants.QUIT:        this.HandleLeaveReceived (ea);		break;
				case P2PConstants.LIST_PEERS:  this.HandleListPeerReceived (ea);	break;
				case P2PConstants.PEERS:       this.HandlePeerListReceived (ea);	break;
				case P2PConstants.PEER_JOINED: this.HandlePeerJoinedReceived (ea);	break;
				case P2PConstants.PEER_QUIT:   this.HandlePeerLeftReceived (ea);	break;
				case P2PConstants.START:       this.HandleGameStartReceived (ea);	break;
			}
			this.OnPacketReceived(this, e);
		}
		
		private void HandleJoinReceived (P2PCommEventArgs e) {
			this.AddPeer (e.peer);
			this.PeerJoined?.Invoke (this, new PeerJoinedEventArgs (e.peer));
			this.OnPacketReceived(this, new ConnectionEventArgs(0, e.peer, System.Text.Encoding.UTF8.GetBytes(e?.data.ToString() ?? string.Empty)));
		}
		
		private void HandleBombReceived (P2PCommEventArgs e) {
			if (
				!e.data.TryGetProperty ("bombtime", out JsonElement json)
				|| json.ValueKind != JsonValueKind.Number
				|| !json.TryGetInt32 (out int bombtime)
			) {
				return;
			}
			this.BombReceived?.Invoke (this, new BombReceivedEventArgs (bombtime));
			this.OnPacketReceived(this, new ConnectionEventArgs(0, e.peer, System.Text.Encoding.UTF8.GetBytes(e?.data.ToString() ?? string.Empty)));
		}

		private void HandleGameStartReceived (P2PCommEventArgs e) {
			if (
				!e.data.TryGetProperty ("bombtime", out JsonElement json)
				|| json.ValueKind != JsonValueKind.Number
				|| !json.TryGetInt32 (out int bombtime)
			) {
				return;
			}
			this.GameStarts?.Invoke (this, new GameStartEventArgs (bombtime));
			this.OnPacketReceived(this, new ConnectionEventArgs(0, e.peer, System.Text.Encoding.UTF8.GetBytes(e?.data.ToString() ?? string.Empty)));
		}

		private void HandleLeaveReceived (P2PCommEventArgs e) {
			this.RemovePeer (e.peer);
			this.PeerLeft?.Invoke (this, new PeerLeftEventArgs (e.peer));
			this.OnPacketReceived(this, new ConnectionEventArgs(0, e.peer, System.Text.Encoding.UTF8.GetBytes(e?.data.ToString() ?? string.Empty)));
		}

		private void HandlePeerLostReceived (P2PCommEventArgs e) {
			this.RemovePeer (e.peer);
			this.PeerLost?.Invoke (this, new PeerLostEventArgs (e.peer));
			this.OnPacketReceived(this, new ConnectionEventArgs(0, e.peer, System.Text.Encoding.UTF8.GetBytes(e?.data.ToString() ?? string.Empty)));
		}

		private void HandlePeerListReceived (P2PCommEventArgs e) {
			if (
				!e.data.TryGetProperty ("peerlist", out JsonElement json)
				|| json.ValueKind != JsonValueKind.Array
			) {
				return;
			}
			int          count = json.GetArrayLength ();
			IPEndPoint[] p     = new IPEndPoint[count];
			int          i     = 0;
			foreach (JsonElement item in json.EnumerateArray ()) {
				if (
					item.ValueKind != JsonValueKind.String
					|| !IPEndPoint.TryParse (item.GetString () ?? string.Empty, out IPEndPoint? peer)
				) {
					continue;
				}
				this.AddPeer (peer);
				p [i] = peer;
				++i;
			}
			this.PeerListReceived?.Invoke (this, new PeerListReceivedEventArgs (p));
			this.OnPacketReceived(this, new ConnectionEventArgs(0, e.peer, System.Text.Encoding.UTF8.GetBytes(e?.data.ToString() ?? string.Empty)));
		}

		private void HandlePeerJoinedReceived (P2PCommEventArgs e) {
			if (
				!e.data.TryGetProperty ("peer", out JsonElement json)
				|| json.ValueKind != JsonValueKind.String
				|| !IPEndPoint.TryParse (json.GetString () ?? string.Empty, out IPEndPoint? peer)
			) {
				return;
			}
			this.AddPeer (peer);
			this.PeerJoined?.Invoke (this, new PeerJoinedEventArgs (peer));
			this.OnPacketReceived(this, new ConnectionEventArgs(0, e.peer, System.Text.Encoding.UTF8.GetBytes(e?.data.ToString() ?? string.Empty)));
		}

		private void HandlePeerLeftReceived (P2PCommEventArgs e) {
			if (
				!e.data.TryGetProperty ("peer", out JsonElement json)
				|| json.ValueKind != JsonValueKind.String
				|| !IPEndPoint.TryParse (json.GetString () ?? string.Empty, out IPEndPoint? peer) 
			) {
				return;
			}
			this.RemovePeer (peer);
			this.PeerLeft?.Invoke (this, new PeerLeftEventArgs (peer));
			this.OnPacketReceived(this, new ConnectionEventArgs(0, e.peer, System.Text.Encoding.UTF8.GetBytes(e?.data.ToString() ?? string.Empty)));
		}

		private void HandleListPeerReceived (P2PCommEventArgs e) {
			// TODO: Should there be event based handling for this elsewhere?
			this.DoSendPeerList (e.peer);
			this.OnPacketReceived(this, new ConnectionEventArgs(0, e.peer, System.Text.Encoding.UTF8.GetBytes(e?.data.ToString() ?? string.Empty)));
		}
		
#endregion

#region Utility
		
		public PeerInfo? GetRandomPeer () {
			const int MAX_RETRIES = 3;
			PeerInfo? bombTarget  = null;
			Random    r           = new Random ();
			int       i           = 0;
			lock (this.peersLock) {
				if (this.peers.Count == 0) {
					return null;
				}
				do {
					int index = r.Next (0, this.peers.Count);
					try {
						bombTarget = this.peers [index];
					} catch {
					}
					++i;
				} while (bombTarget == null && i < MAX_RETRIES);
			}
			return bombTarget;
		}

		private void AddPeer (IPEndPoint peerIp) {
			lock (this.peersLock) {
				PeerInfo peer = new PeerInfo (peerIp);
				if (!this.peers.Contains (peer, PeerInfoComparer.instance)) {
					this.peers.Add (peer);
				}
			}
		}

		public void RemovePeer (IPEndPoint peerIp) {
			PeerInfo         peer    = new PeerInfo (peerIp);
			PeerInfoComparer compare = PeerInfoComparer.instance;
			lock (this.peersLock) {
				for (int i = 0, count = this.peers.Count; i < count; ++i) {
					if (compare.Equals (this.peers [i], peer)) {
						this.peers.RemoveAt (i);
						break;
					}
				}
			}
		}

		private void ClearPeers () {
			lock (this.peersLock) {
				this.peers.Clear ();
			}
		}

		static private byte[] encodeMsg (Msg data) {
			return System.Text.Encoding.UTF8.GetBytes (JsonSerializer.Serialize (data));
		}

		static private Msg? decodeMsg (byte[] msg) {
			return JsonSerializer.Deserialize<Msg> (System.Text.Encoding.UTF8.GetString (msg));
		}

		static JsonElement objectToJson (object obj) {
			byte[]             bytes = JsonSerializer.SerializeToUtf8Bytes (obj);
			using JsonDocument doc   = JsonDocument.Parse (bytes);
			return doc.RootElement.Clone();
		}

#endregion
		
	}

}
