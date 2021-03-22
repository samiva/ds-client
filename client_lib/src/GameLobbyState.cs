using System;
using System.Collections.Generic;

namespace BombPeliLib
{
	public class GameLobbyState : State
	{

		public delegate void StartGameEventHandler (object sender, StartGameEventArgs e);
		public delegate void LeaveGameLobbyEventHandler (object sender, LeaveGameLobbyEventArgs e);
		public event StartGameEventHandler StartGame;
		public event LeaveGameLobbyEventHandler LeaveLobby;

		private Config config;
		private GameInfo gameInfo;
		private P2Pplayer client;

		public GameLobbyState (GameInfo gi, Config config, P2Pplayer p2p) {
			this.gameInfo = gi;
			this.config = config;
			this.client = p2p;
			if (client.IsHost) {
				client.PeerJoined += PeerJoinedHandler;
				client.PeerQuit += PeerLeftHandler;
			} else {
				client.GameStartReceived += StartGameHandler;
				client.PeerQuit += LeaveGameLobbyHandler;
			}
		}

		~GameLobbyState () {
			Destroy (true);
		}

		public List<PeerInfo> Peers {
			get {
				return client.Peers;
			}
		}

		public bool IsHost {
			get {
				return client.IsHost;
			}
		}

		public GameInfo Game {
			get {
				return gameInfo;
			}
		}

		public void InvokeStartGame () {
			StartGame?.Invoke (this, new StartGameEventArgs (this));
		}

		public void InvokeLeaveLobby () {
			LeaveLobby?.Invoke (this, new LeaveGameLobbyEventArgs (this));
		}

		public void DoStartGame () {
			if (!client.IsHost) {
				return;
			}
			foreach (PeerInfo p in client.Peers) {
				client.SendStartGame (p.Address, p.Port);
			}
			Destroy (false);
		}

		public void DoLeaveLobby () {
			if (client?.IsHost == true) {
				TerminateGame ();
			} else {
				client.SendQuitGame (gameInfo.Ip, gameInfo.Port);
			}
			Destroy (true);
		}

		public void PeerJoinedHandler (object sender, P2PCommEventArgs e) {
			client.BroadcastPeerJoin (e.RemoteAddress, e.RemotePort);
		}

		public void PeerLeftHandler (object sender, P2PCommEventArgs e) {
			client.BroadcastPeerQuit (e.RemoteAddress, e.RemotePort);
		}

		public void StartGameHandler (object sender, P2PCommEventArgs e) {
			InvokeStartGame ();
		}

		public void LeaveGameLobbyHandler (object sender, P2PCommEventArgs e) {
			InvokeLeaveLobby ();
		}

		private void TerminateGame () {
			foreach (PeerInfo p in client.Peers) {
				client.SendQuitGame (p.Address, p.Port);
			}
			ServiceDiscoveryClient service = new ServiceDiscoveryClient (config);
			service.DeregisterGame (gameInfo);
		}

		private void Destroy (bool closeClient) {
			// Ensure that no references to P2P client are left here.
			P2Pplayer c = client;
			if (c == null) {
				return;
			}
			if (c.IsHost) {
				c.PeerJoined -= PeerJoinedHandler;
				c.PeerQuit -= PeerLeftHandler;
			}
			if (closeClient) {
				client.Close ();
			}
			client = null;
		}
	}
}