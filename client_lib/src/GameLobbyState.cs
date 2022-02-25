using System;
using System.Collections.Generic;
using System.Net;
using BombPeliLib.Events;

namespace BombPeliLib
{
	public class GameLobbyState : State
	{

		readonly private Config   config;
		private          GameInfo gameInfo;
		private          P2PApi?  client;
		private          int      bombTime;

		public GameLobbyState (GameInfo gi, Config config, P2PApi p2p) {
			this.gameInfo = gi;
			this.config   = config;
			this.client   = p2p;
			this.bombTime = config.GetInt ("bomb_lifetime");
		}

		~GameLobbyState () {
			this.Destroy (true);
		}

		public List<PeerInfo> Peers {
			get { return this.client?.Peers ?? new List<PeerInfo> (); }
		}

		public bool IsHost {
			get { return this.client?.IsHost ?? false; }
		}

		public GameInfo Game {
			get { return this.gameInfo; }
		}

		public int BombTime {
			get { return this.bombTime; }
		}

		public void LeaveLobby () {
			if (this.IsHost == true) {
				this.TerminateGame ();
			} else {
				this.client?.DoLeaveGame (this.gameInfo.getEndpoint ());
			}
			this.Destroy (true);
		}

		public void PeerJoinedHandler (object sender, PeerJoinedEventArgs e) {
			this.client?.DoBroadcastPeerJoined (e.peer);
		}

		public bool IsHostPeer (IPEndPoint ip) {
			return this.gameInfo.getEndpoint ().Equals (ip);
		}

		public void PeerLeftHandler (object sender, PeerLeftEventArgs e) {
			if (this.IsHost) {
				this.client?.BroadcastPeerQuit (e.peer);
			}
		}

		public void StartGameHandler (object sender, GameStartEventArgs e) {
			this.bombTime = e.bombtime;
			this.Destroy (false);
		}

		private void TerminateGame () {
			ServiceDiscoveryClient service = new ServiceDiscoveryClient (this.config);
			service.DeregisterGame (this.gameInfo);
			P2PApi? c = this.client;
			if (c == null) {
				return;
			}
			foreach (PeerInfo p in c.Peers) {
				c.DoLeaveGame (p.ip);
			}
		}

		private void Destroy (bool closeClient) {
			// Ensure that no references to P2P client are left here.
			P2PApi? c = this.client;
			if (c == null) {
				return;
			}
			if (closeClient) {
				c.Close ();
			}
			this.client = null;
		}

	}
}