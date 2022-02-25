using BombPeliLib;

using System;
using System.Threading;
using BombPeliLib.Events;

namespace BombBot
{
    public class GameLobby
	{
		
        private GameLobbyState lobby;
		private Config         config;
		private EventWaitHandle playerWaitTask;
		private bool           isHost;
		private int            playerCapacity;
		private bool           success = false;

        public GameLobby (GameInfo game, Config config, P2PApi client, bool isHost, int playerCapacity) {
			this.isHost         = isHost;
			this.playerCapacity = playerCapacity;
			this.config         = config;
			this.lobby          = new GameLobbyState (game, config, client);
			this.playerWaitTask = new EventWaitHandle (false, EventResetMode.ManualReset);
		}

		public GameLobbyState getState {
			get { return this.lobby; }
		}

		public void init (P2PApi client) {
			client.GameStarts       += this.StartGameHandler;
        	client.PeerListReceived += this.PeerListChangedHandler;
        	client.PeerJoined       += this.PeerListChangedHandler;
        	client.PeerLeft         += this.PeerListChangedHandler;
			client.PeerLeft         += this.LobbyPeerLeftHandler;
        	if (client.IsHost) {
        		client.PeerJoined += this.lobby.PeerJoinedHandler;
        	} else {
        		client.GameStarts += this.lobby.StartGameHandler;
        	}
		}

        public void release (P2PApi client) {
			client.GameStarts       -= this.StartGameHandler;
        	client.PeerListReceived -= this.PeerListChangedHandler;
        	client.PeerJoined       -= this.PeerListChangedHandler;
        	client.PeerLeft         -= this.PeerListChangedHandler;
			client.PeerLeft         -= this.LobbyPeerLeftHandler;
			if (client.IsHost) {
        		client.PeerJoined -= this.lobby.PeerJoinedHandler;
        	} else {
        		client.GameStarts -= this.lobby.StartGameHandler;
        	}
        }

		public bool waitForPlayers () {
			this.playerWaitTask.WaitOne ();
			return this.success;
		}

		public void PeerListChangedHandler(object? sender, EventArgs e) {
			if (!this.isHost) {
				return;
			}
			if (this.lobby.Peers.Count >= this.playerCapacity) {
				this.success = true;
				this.playerWaitTask.Set ();
			}
		}
		
		private void StartGameHandler (object? sender, GameStartEventArgs e) {
			this.success = true;
			this.playerWaitTask.Set ();
		}

		private void LeaveGameLobbyHandler (object? sender, EventArgs e) {
			try {
				this.lobby.LeaveLobby ();
			} catch (Exception ex) {
				Console.WriteLine ("Failed to leave game lobby.\n{0}\n\n{1}", ex.Message, ex.StackTrace);
			}
			this.playerWaitTask.Set ();
		}

		public void LobbyPeerLeftHandler (object? sender, PeerLeftEventArgs e) {
			this.lobby.PeerLeftHandler (sender, e);
			if (this.lobby.IsHostPeer (e.peer)) {
				// Host left, leave lobby.
				this.LeaveGameLobbyHandler (sender, e);
			}
		}
		
    }
}
