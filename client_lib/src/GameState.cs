using System;
using System.Collections.Generic;

namespace BombPeliLib
{
	public class GameState : State
	{
		private Config config;
		private P2Pplayer client;
		private bool hasBomb = false;
		private long bombTime;

		public GameState (Config config, P2Pplayer p2p) {
			this.config = config;
			this.client = p2p;
			this.bombTime = config.GetLong ("bomb_lifetime");
			client.BombReceived += ReceiveBombHandler;
		}

		~GameState () {
			Destroy ();
		}

		public void ReceiveBombHandler (object sender, EventArgs e) {
			hasBomb = true;
		}

		public void PassBomb () {
			if (!hasBomb) {
				return;
			}
			int i = 0;
			const int MAX_RETRIES = 3;
			PeerInfo? bombTarget;
			do {
				bombTarget = GetRandomPeer ();
				++i;
			} while (bombTarget == null && i < MAX_RETRIES);
			if (bombTarget == null) {
				return;
			}
			client.SendBomb (bombTarget.Value.Address, bombTarget.Value.Port);
			hasBomb = false;
		}

		public void LeaveGame () {
			if (client.IsHost) {
				client.BroadcastPeerQuit (client.Game.Ip, client.Game.Port);
			} else {
				client.SendQuitGame (client.Game.Ip, client.Game.Port);
			}
			Destroy ();
		}

		public void FailPassBomb () {
			hasBomb = true;
		}

		private void Destroy () {
			if (client == null) {
				return;
			}
			client = null;
		}

		private PeerInfo? GetRandomPeer () {
			List<PeerInfo> peers = client.Peers;
			Random r = new Random();
			int index = r.Next (0, peers.Count - 1);
			try {
				return peers [index];
			} catch {
			}
			return null;
		}
	}
}