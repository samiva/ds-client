using System;
using System.Collections.Generic;

namespace BombPeliLib
{
	public class GameLobbyState : State
	{

		private Config config;
		private GameInfo gameInfo;
		private P2Pplayer client;
		private int bombTime;

		public GameLobbyState (GameInfo gi, Config config, P2Pplayer p2p) {
			this.gameInfo = gi;
			this.config = config;
			this.client = p2p;
			this.bombTime = config.GetInt ("bomb_lifetime");
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

		public int BombTime {
			get {
				return bombTime;
			}
		}

		public void LeaveLobby () {
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

		public bool IsHostPeer (string address, int port) {
			return address == gameInfo.Ip && port == gameInfo.Port;
		}

		public void PeerLeftHandler (object sender, P2PCommEventArgs e) {
			if (client.IsHost) {
				client.BroadcastPeerQuit (e.RemoteAddress, e.RemotePort);
			}
		}

		public void StartGameHandler (object sender, GameStartEventArgs e) {
			this.bombTime = e.bombTime;
			Destroy (false);
		}

		private void TerminateGame () {
			ServiceDiscoveryClient service = new ServiceDiscoveryClient (config);
			service.DeregisterGame (gameInfo);
			foreach (PeerInfo p in client.Peers) {
				client.SendQuitGame (p.Address, p.Port);
			}
		}

		private void Destroy (bool closeClient) {
			// Ensure that no references to P2P client are left here.
			P2Pplayer c = client;
			if (c == null) {
				return;
			}
			if (closeClient) {
				client.Close ();
			}
			client = null;
		}

	}
}