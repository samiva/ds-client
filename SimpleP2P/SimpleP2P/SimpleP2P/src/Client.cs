using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleP2P
{
	sealed internal class Client
	{

		readonly private P2PManager              p2pman;
		readonly private UdpClient               client;
		readonly private CancellationTokenSource taskToken;
		private          Task?                   action;
		private          bool                    closed;
		
		public Client (P2PManager p2pman, ushort port) {
			const uint IOC_IN     = 0x80000000;
			const uint IOC_VENDOR = 0x18000000;
			const uint SIO_UDP_CONNRESET =
				IOC_IN | IOC_VENDOR | (uint)IOControlCode.KeepAliveValues | (uint)IOControlCode.BindToInterface;
			this.p2pman    = p2pman;
			this.client    = new UdpClient (new IPEndPoint (IPAddress.Any, port));
			// MEMO: Set IOCTL control code to disable ICMP "connection lost" exceptions
			this.client.Client.IOControl (unchecked ((int)SIO_UDP_CONNRESET), new byte []{0}, null);
			this.closed    = false;
			this.taskToken = new CancellationTokenSource ();
		}

		~Client () {
			this.stop ();
		}

		public void send (IPEndPoint peer, byte[] msg) {
			try {
				this.client.Send (msg, msg.Length, peer);
			} catch (Exception) {
				if (!this.closed) {
					throw;
				}
			}
		}

		public void run () {
			this.action = Task.Run (this.routine, this.taskToken.Token);
		}

		public void stop () {
			this.closed = true;
			this.taskToken.Cancel ();
			this.client.Close ();
		}

		private void routine () {
			while (!this.taskToken.IsCancellationRequested && this.receiveMsg ());
		}

		private bool receiveMsg () {
			IPEndPoint? sender = null;
			byte[]      msg;
			try {
				msg = this.client.Receive (ref sender);
			} catch (SocketException) {
				if (this.closed) {
					return true;
				}
				this.p2pman.OnSocketDied (this, EventArgs.Empty);
				return false;
			}
			if (msg.Length > 0) {
				try {
					this.p2pman.OnRawMsg (this, new CommunicationEventArgs (sender, msg));
				} catch {
					// MEMO: External msg handling code throwing.
				}
			}
			return true;
		}

	}
}
