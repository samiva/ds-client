using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace BombPeliLib
{
	public class P2Pplayer
	{

		public event EventHandler<P2PCommEventArgs> GameStartReceived;
		public event EventHandler<P2PCommEventArgs> GameEndReceived;
		public event EventHandler<P2PCommEventArgs> BombReceived;
		public event EventHandler<P2PCommEventArgs> PeerQuit;
		public event EventHandler<P2PCommEventArgs> PeerJoined;
		public event EventHandler<P2PCommEventArgs> PeerListReceived;

		private readonly P2PComm p2p;
		private GameInfo currentGame;
		private List<PeerInfo> peers;
		private bool isHost;

		private object peersLock = new object ();

		public P2Pplayer (P2PComm p2p, bool isHost, GameInfo currentGame) {
			this.p2p = p2p;
			this.p2p.DataReceived += P2p_DataReceived;
			this.peers = new List<PeerInfo> ();
			this.isHost = isHost;
			this.currentGame = currentGame;
		}

		public List<PeerInfo> Peers {
			get {
				lock (peersLock) {
					return peers;
				}
			}
			private set {
				lock (peersLock) {
					peers = value;
				}
			}
		}

		public bool IsHost {
			get {
				return isHost;
			}
		}

		public GameInfo Game {
			get {
				return currentGame;
			}
		}

		private void P2p_DataReceived (object sender, P2PCommEventArgs e) {
			switch (e.MessageChannel) {
				case Channel.GAME:			ProcessGameDataMsg (e);			break;
				case Channel.MANAGEMENT:	ProcessManagementDataMsg (e);	break;
			}
		}

		private void ProcessGameDataMsg (P2PCommEventArgs e) {
			GameStatus status = (GameStatus)e.Data.status;
			bool bomb = (bool)e.Data.bomb;

			if (bomb) {
				OnBombReceived (e);
			} else {
				switch (status) {
					case GameStatus.RUNNING:	OnGameStartReceived (e);	break;
					case GameStatus.ENDED:		OnGameEndReceived (e);		break;
				}
			}
		}

		private void ProcessManagementDataMsg (P2PCommEventArgs e) {
			JObject data;
			string msg;
			try {
				data = e.Data;
				msg = data.GetValue ("msg").Value<string> ().ToLower ();
			} catch {
				return;
			}
			switch (msg) {
				case "join":		OnJoinReceived (e);				break;
				case "quit":		OnQuitReceived (e);				break;
				case "list_peers":	OnListPeers (e);				break;
				case "peers":		OnPeerListReceived (e, data);	break;
				case "peer_joined":	OnPeerJoinedReceived (e, data);	break;
				case "peer_quit":	OnPeerQuitReceived (e, data);	break;
			}
		}

		public void SendBomb (string address, int port) {
			p2p.Send (Channel.GAME, new {
				bomb = true,
				status = GameStatus.RUNNING
			}, address, port);
		}

		public void SendStartGame (string address, int port) {
			p2p.Send (Channel.GAME, new {
				bomb = false,
				status = GameStatus.RUNNING
			}, address, port);
		}

		// Send when game is won and ends
		public void SendEndGame (string address, int port) {
			p2p.Send (Channel.GAME, new {
				bomb = false,
				status = GameStatus.ENDED
			}, address, port);
		}

		public void SendJoinGame (string address, int port) {
			p2p.Send (Channel.MANAGEMENT, new {
				msg = "join"
			}, address, port);
			Peers.Add (new PeerInfo (address, port));
		}

		// Send when client leaves network
		public void SendQuitGame (string address, int port) {
			p2p.Send (Channel.MANAGEMENT, new {
				msg = "quit"
			}, address, port);
		}

		public void SendListPeers (string address, int port) {
			p2p.Send (Channel.MANAGEMENT, new {
				msg = "list_peers"
			}, address, port);
		}

		public void BroadcastPeerJoin (string address, int port) {
			PeerInfoComparer compare = PeerInfoComparer.instance;
			PeerInfo peer = new PeerInfo (address, port);
			object msg = new {
				msg = "peer_joined",
				peer = JsonConvert.SerializeObject(peer, Formatting.None)
			};
			foreach (PeerInfo p in Peers) {
				if (!compare.Equals (p, peer)) {
					p2p.Send (Channel.MANAGEMENT, msg, p.Address, p.Port);
				}
			}
		}

		public void BroadcastPeerQuit (string address, int port) {
			PeerInfoComparer compare = PeerInfoComparer.instance;
			PeerInfo peer = new PeerInfo (address, port);
			object msg = new {
				msg = "peer_quit",
				peer = JsonConvert.SerializeObject(peer, Formatting.None)
			};
			foreach (PeerInfo p in Peers) {
				if (!compare.Equals (p, peer)) {
					p2p.Send (Channel.MANAGEMENT, msg, p.Address, p.Port);
				}
			}
		}

		public void Close () {
			p2p.Close ();
		}

		private void OnGameStartReceived (P2PCommEventArgs e) {
			GameStartReceived?.Invoke (this, e);
		}

		private void OnBombReceived (P2PCommEventArgs e) {
			BombReceived?.Invoke (this, e);
		}

		private void OnJoinReceived (P2PCommEventArgs e) {
			AddPeer (e.RemoteAddress, e.RemotePort);
			PeerJoined?.Invoke (this, e);
		}

		private void OnQuitReceived (P2PCommEventArgs e) {
			RemovePeer (e.RemoteAddress, e.RemotePort);
			PeerQuit?.Invoke (this, e);
		}

		private void OnGameEndReceived (P2PCommEventArgs e) {
			GameEndReceived?.Invoke (this, e);
		}

		private void OnPeerListReceived (P2PCommEventArgs e, JObject data) {
			string peerList = data.GetValue("peerlist").Value<string>();
			List<PeerInfo> p = JsonConvert.DeserializeObject<List<PeerInfo>> (peerList);
			foreach (PeerInfo peer in p) {
				AddPeer (peer);
			}
			PeerListReceived?.Invoke (this, e);
		}

		private void OnPeerJoinedReceived (P2PCommEventArgs e, JObject data) {
			string peerStr = data.GetValue ("peer").Value<string> ();
			PeerInfo peer = JsonConvert.DeserializeObject<PeerInfo> (peerStr);
			AddPeer (peer.Address, peer.Port);
			PeerJoined?.Invoke (this, e);
		}

		private void OnPeerQuitReceived (P2PCommEventArgs e, JObject data) {
			string peerStr = data.GetValue ("peer").Value<string> ();
			PeerInfo peer = JsonConvert.DeserializeObject<PeerInfo> (peerStr);
			RemovePeer (peer.Address, peer.Port);
			PeerQuit?.Invoke (this, e);
		}

		private void AddPeer (string address, int port) {
			AddPeer (new PeerInfo(address, port));
		}

		private void AddPeer (PeerInfo peer) {
			if (!Peers.Contains (peer, PeerInfoComparer.instance)) {
				Peers.Add (peer);
			}
		}

		private void RemovePeer (string address, int port) {
			PeerInfoComparer compare = PeerInfoComparer.instance;
			PeerInfo tmp = new PeerInfo(address, port);
			for (
				int i = 0, count = Peers.Count;
				i < count;
				++i
			) {
				if (compare.Equals (Peers[i], tmp)) {
					Peers.RemoveAt (i);
					break;
				}
			}
		}

		private void OnListPeers (P2PCommEventArgs e) {
			PeerInfoComparer compare = PeerInfoComparer.instance;
			List<PeerInfo> peerList = new List<PeerInfo> ();
			PeerInfo peer = new PeerInfo (e.RemoteAddress, e.RemotePort);
			foreach (PeerInfo p in Peers) {
				if (!compare.Equals (p, peer)) {
					peerList.Add (p);
				}
			}
			string json = JsonConvert.SerializeObject(peerList, Formatting.None);
			p2p.Send (Channel.MANAGEMENT, new {
				msg = "peers",
				peerlist = json
			}, e.RemoteAddress, e.RemotePort);
		}

	}
}
