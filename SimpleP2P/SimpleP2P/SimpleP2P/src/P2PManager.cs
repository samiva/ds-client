using System;
using System.Net;
using System.Threading.Tasks;

namespace SimpleP2P
{
	sealed public class P2PManager
	{
		/*
			- Management messages
			- Data messages
		*/
		
		public event EventHandler<ConnectionEventArgs>? GreetSucceeded;
		public event EventHandler<ConnectionEventArgs>? GreetFailed;
		public event EventHandler<ConnectionEventArgs>? PeerGreets;
		
		public event EventHandler<ConnectionEventArgs>? FarewellSucceeded;
		public event EventHandler<ConnectionEventArgs>? FarewellFailed;
		public event EventHandler<ConnectionEventArgs>? PeerFarewells;

		public event EventHandler<ConnectionEventArgs>? SendSucceeded;
		public event EventHandler<ConnectionEventArgs>? SendFailed;
		public event EventHandler<ConnectionEventArgs>? MessageReceived;

		public event EventHandler<EventArgs>? SocketDied;
		
		readonly private Client    client;
		readonly private Messenger messenger;
		
		public P2PManager (ushort port) {
			this.client    = new Client (this, port);
			this.messenger = new Messenger (this, this.client);
		}

		~P2PManager () {
			this.end ();
		}

		public void start () {
			this.client.run ();
			this.messenger.runFailTask ();
		}

		public void end () {
			this.client.stop ();
			this.messenger.endFailTask ();
		}

		public long send (IPEndPoint peer, byte[] msg) {
			return this.messenger.sendMsg (peer, msg);
		}

		public long greet (IPEndPoint peer) {
			return this.messenger.greetPeer (peer);
		}

		public long farewell (IPEndPoint peer) {
			return this.messenger.farewellPeer (peer);
		}

		internal void OnRawMsg (object? sender, CommunicationEventArgs e) {
			this.messenger.handle (e.peer, e.msg);
		}

		internal void OnSocketDied (object? sender, EventArgs e) {
			this.SocketDied?.Invoke (this, EventArgs.Empty);
		}

		internal void OnPeerGreets (object? sender, ConnectionEventArgs e) {
			this.PeerGreets?.Invoke (this, e);
		}

		internal void OnGreetSucceeded (object? sender, ConnectionEventArgs e) {
			this.GreetSucceeded?.Invoke (this, e);
		}

		internal void OnGreetFailed (object? sender, ConnectionEventArgs e) {
			this.GreetFailed?.Invoke (this, e);
		}

		internal void OnFarewellSucceeded (object? sender, ConnectionEventArgs e) {
			this.FarewellSucceeded?.Invoke (this, e);
		}

		internal void OnFarewellFailed (object? sender, ConnectionEventArgs e) {
			this.FarewellFailed?.Invoke (this, e);
		}
		
		internal void OnPeerFarewells (object? sender, ConnectionEventArgs e) {
			this.PeerFarewells?.Invoke (this, e);
		}
		
		internal void OnMsg (object? sender, ConnectionEventArgs e) {
			this.MessageReceived?.Invoke (this, e);
		}

		internal void OnSendSucceeded (object? sender, ConnectionEventArgs e) {
			this.SendSucceeded?.Invoke (this, e);
		}

		internal void OnSendFailed (object? sender, ConnectionEventArgs e) {
			this.SendFailed?.Invoke (this, e);
		}
	}
}
