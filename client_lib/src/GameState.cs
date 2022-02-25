using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using BombPeliLib.Events;

namespace BombPeliLib
{
	public class GameState : State
	{
		readonly private Config   config;
		private          P2PApi?  client;
		private          GameInfo game;
		private          bool     hasBomb = false;
		private          int      originalBombTime;
		private          int      bombTime;
		private          long     startTime;

		public GameState (Config config, P2PApi p2p, GameInfo game, int bombTime) {
			this.config           = config;
			this.client           = p2p;
			this.game             = game;
			this.bombTime         = bombTime;
			this.originalBombTime = bombTime;
			this.startTime        = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds ();
			if (this.client.IsHost) {
				this.hasBomb = true;
			}
		}

		~GameState () {
			this.Destroy ();
		}

		public P2PApi? Client {
			get {
				return this.client;
			}
		}

		public bool HasBomb {
			get {
				return this.hasBomb;
			}
		}

		public int OriginalBombTime {
			get {
				return this.originalBombTime;
			}
		}

		public void StartGame () {
			if (this.client?.IsHost != true) {
				return;
			}
			foreach (PeerInfo p in this.client.Peers) {
				this.client.DoStartGame (p.ip, this.bombTime);
			}

		}

		public void ResetBombTimes () {
			this.startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds ();
		}

		public bool ReceiveBomb (int bombTime) {
			this.hasBomb = true;
			return true;
		}

		public void SendBombFailedHandler (object sender, EventArgs e) {
			this.FailPassBomb ();
		}

		public void PeerLeftHandler (object sender, PeerLeftEventArgs e) {
			if (this.client?.IsHost == true) {
				this.client.BroadcastPeerQuit (e.peer);
			}
		}

		public bool IsWinner () {
			using IEnumerator<PeerInfo> e = (this.client?.Peers ?? new List<PeerInfo> ()).GetEnumerator ();
			return !e.MoveNext ();
		}

		public void Win() {
			this.hasBomb = false;
			this.Destroy();
		}

		public void Lose () {
			this.client?.DoBroadcastLose ();
			this.RespawnBomb();
			this.Destroy();
		}

		public void LeaveGame() {
			if (this.client?.IsHost == true) {
				this.client?.BroadcastPeerQuit(this.game.getEndpoint ());
			} else {
				this.client?.DoLeaveGame(this.game.getEndpoint ());
			}
			this.Destroy();
		}

		public void PassBomb (PeerInfo peer, int bombtime) {
			if (!this.hasBomb) {
				return;
			}
			try {
				this.client?.DoSendBomb (peer.ip, bombtime);
				this.hasBomb = false;
			} catch (SocketException) {
				// Likely dropped connection. Remove from target list.
				this.client?.RemovePeer (peer.ip);
				throw;
			}
		}

		public void FailPassBomb () {
			this.hasBomb = true;
		}

		private void RespawnBomb() {
			PeerInfo? peer = this.client?.GetRandomPeer();
			if (!peer.HasValue || this.client?.Peers.Count () <= 1) {
				return;
			}
			try {
				this.client?.DoSendBomb (peer.Value.ip, this.originalBombTime);
			} catch {
				throw;
			}
			this.hasBomb = false;
		}

		private void Destroy () {
			P2PApi? c = this.client;
			if (c == null) {
				return;
			}
			c.Close ();
			this.client = null;
		}

		public int getRemainingBombTime () {
			long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds ();
			return this.bombTime - (int)(now - this.startTime);
		}
		
	}
}