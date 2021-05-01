using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace BombPeliLib
{
	public class GameState : State
	{
		private Config config;
		private P2Pplayer client;
		private GameInfo game;
		private bool hasBomb = false;
		private int originalBombTime;
		private int bombTime;
		private long startTime;

		public GameState (Config config, P2Pplayer p2p, GameInfo game, int bombTime) {
			this.config = config;
			this.client = p2p;
			this.game = game;
			this.bombTime = bombTime;
			this.originalBombTime = bombTime;
			this.startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds ();
			if (this.client.IsHost) {
				this.hasBomb = true;
			}
		}

		~GameState () {
			Destroy ();
		}

		public P2Pplayer Client {
			get {
				return client;
			}
		}

		public bool HasBomb {
			get {
				return hasBomb;
			}
		}

		public int OriginalBombTime {
			get {
				return originalBombTime;
			}
		}

		public void StartGame () {
			if (client.IsHost) {
				foreach (PeerInfo p in client.Peers) {
					client.SendStartGame (p.Address, p.Port, bombTime);
				}
			}
		}

		public void ResetBombTimes () {
			startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds ();
		}

		public bool ReceiveBomb (int bombTime) {
			hasBomb = true;
			return true;
		}

		public void SendBombFailedHandler (object sender, EventArgs e) {
			FailPassBomb ();
		}

		public void PeerLeftHandler (object sender, P2PCommEventArgs e) {
			if (client.IsHost) {
				client.BroadcastPeerQuit (e.RemoteAddress, e.RemotePort);
			}
		}

		public bool IsWinner () {
			return client.Peers.Count == 0;
		}

		public void Win() {
			hasBomb = false;
			Destroy();
		}

		public void Lose () {
			client.BroadcastLose ();
			System.Threading.Thread.Sleep(500);
			RespawnBomb();
			Destroy();
		}

		public void LeaveGame() {
			if (client?.IsHost == true) {
				client?.BroadcastPeerQuit(game.Ip, game.Port);
			} else {
				client?.SendQuitGame(game.Ip, game.Port);
			}
			Destroy();
		}

		public void PassBomb (PeerInfo peer, int bombtime) {
			if (!hasBomb) {
				return;
			}
			try {
				client.SendBomb (peer.Address, peer.Port, bombtime);
				hasBomb = false;
			} catch (SocketException ex) {
				// Likely dropped connection. Remove from target list.
				client.RemovePeer (peer);
				throw;
			}
		}

		public void FailPassBomb () {
			hasBomb = true;
		}

		private void RespawnBomb() {
			PeerInfo? peer = client.GetRandomPeer();
			if (!peer.HasValue || client.Peers.Count <= 1) {
				return;
			}
			try {
				client.SendBomb (peer.Value.Address, peer.Value.Port, originalBombTime);
			} catch {
				throw;
			}
			hasBomb = false;
		}

		private void Destroy () {
			P2Pplayer c = client;
			if (c == null) {
				return;
			}
			c.Close ();
			client = null;
		}

		public int getRemainingBombTime () {
			long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds ();
			return bombTime - (int)(now - startTime);
		}
	}
}