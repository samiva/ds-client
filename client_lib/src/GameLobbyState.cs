using System;
using System.Collections.Generic;

namespace BombPeliLib
{
	public class GameLobbyState {

		private Config config;
		private GameInfo gameInfo;
		private List<PeerInfo> peerInfos;
		private P2Pplayer client;
		private bool isHost;

		public GameLobbyState (GameInfo gi, List<PeerInfo> peers, Config config, P2Pplayer p2p, bool isHost) {
			this.gameInfo = gi;
			this.peerInfos = peers;
			this.config = config;
			this.client = p2p;
			this.isHost = isHost;

			this.client.JoinReceived += HandlePeerJoin;
			this.client.QuitReceived += HandlePeerLeave;
		}

		~GameLobbyState () {
			this.client.JoinReceived -= HandlePeerJoin;
			this.client.QuitReceived -= HandlePeerLeave;
		}

		public List<PeerInfo> Peers {
			get {
				return peerInfos;
			}
		}

		public bool IsHost {
			get {
				return isHost;
			}
		}

		public GameInfo Game {
			get {
				return gameInfo;
			}
		}

		private void P2p_DataReceived (object sender, P2PCommEventArgs e) {
			switch (e.MessageChannel) {
				case Channel.MANAGEMENT:
					if (isHost && peerInfos.Count <= config.GetInt ("maxpeer")) {
						PeerInfo peer = new PeerInfo(e.RemoteAddress, e.RemotePort);
						peerInfos.Add (peer);
					}
					break;
				case Channel.GAME:
					GameStatus status = e.Data.status;
					if (status == GameStatus.RUNNING) {
					} else if (status == GameStatus.ENDED) {
						LeaveGame ();
					}
					break;
			}
		}

		private void HandlePeerJoin (object sender, P2PCommEventArgs e) {
			
		}

		private void HandlePeerLeave (object sender, P2PCommEventArgs e) {

		}

		public void StartGame () {
			if (isHost) {
				foreach (PeerInfo p in peerInfos) {
					client.SendStartGame (p.Address, p.Port);
				}
			}
		}

		public void LeaveGame () {
			if (isHost) {
				TerminateGame ();
			}
			LeaveLobby ();
		}

		private void TerminateGame () {
			foreach (PeerInfo p in peerInfos) {
				client.SendQuitGame (p.Address, p.Port);
			}
			ServiceDiscoveryClient service = new ServiceDiscoveryClient (config);
			service.DeregisterGame (gameInfo);
		}

		private void LeaveLobby () {
			
		}

	}
}