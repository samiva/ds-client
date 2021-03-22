using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using BombPeliLib;

namespace BombPeli
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {

		private Config config;

		private Bootstrap initView;
		private GameList gameListView;
		private ConfigureGame configGameView;
		private GameLobby lobbyView;
		private Game gameView;

		IChangePage currentView;

		public MainWindow () {
			InitializeComponent ();

			initView = new Bootstrap ();
			initView.InitComplete += InitCompleteHandler;
			DisplayPage (initView, null);
		}

		public Config Config {
			get {
				return config;
			}
			private set {
				if (value != config) {
					config = value;
				}
			}
		}

		public void InitCompleteHandler (object sender, EventArgs e) {
			// Read data from init view and release it.
			// Move on to game list.
			config = initView.Config;
			List<GameInfo> games = initView.Games;
			initView = null;

			GameListState gameListState = new GameListState (games, config);
			gameListView = new GameList (gameListState, config);
			gameListView.CreateGame += CreateNewGameHandler;
			gameListView.JoinGame += JoinGameHandler;
			gameListView.Quit += QuitHandler;
			DisplayPage (gameListView, null);
		}

		private void InitLobby (P2Pplayer client, GameLobbyState state) {
			if (lobbyView == null) {
				lobbyView = new GameLobby ();
				lobbyView.StartGame += StartGameHandler;
				lobbyView.LeaveLobby += LeaveGameLobbyHandler;
			}
			client.GameStartReceived += StartGameHandler;
			client.PeerListReceived += lobbyView.PeerListChangedHandler;
			client.PeerJoined += lobbyView.PeerListChangedHandler;
			client.PeerQuit += lobbyView.PeerListChangedHandler;
			client.PeerQuit += LobbyPeerLeftHandler;
			
			if (client.IsHost) {
				client.PeerJoined += state.PeerJoinedHandler;
			} else {
				client.GameStartReceived += state.StartGameHandler;
			}
		}

		private void ReleaseLobby (P2Pplayer client) {
			GameLobbyState state = lobbyView.GetState () as GameLobbyState;
			client.GameStartReceived -= StartGameHandler;
			client.PeerListReceived -= lobbyView.PeerListChangedHandler;
			client.PeerJoined -= lobbyView.PeerListChangedHandler;
			client.PeerQuit -= lobbyView.PeerListChangedHandler;
			client.PeerQuit -= LobbyPeerLeftHandler;

			if (client.IsHost) {
				client.PeerJoined -= state.PeerJoinedHandler;
			} else {
				client.GameStartReceived -= state.StartGameHandler;
			}
		}

		private void InitGame (P2Pplayer client, GameState state) {
			if (gameView == null) {
				gameView = new Game ();
				gameView.LeaveGame += LeaveGameHandler;
				gameView.PassBomb += PassBombHandler;
			}
			client.PeerQuit += state.PeerLeftHandler;
			client.PeerLostReceived += PeerLostGameHandler;
			client.BombReceived += BombReceivedHandler;
			client.BombSendFailed += BombSendFailedHandler;
		}

		private void ReleaseGame (P2Pplayer client) {
			GameState state = gameView.GetState () as GameState;
			client.PeerQuit -= state.PeerLeftHandler;
			client.PeerLostReceived -= PeerLostGameHandler;
			client.BombReceived -= BombReceivedHandler;
			client.BombSendFailed -= BombSendFailedHandler;
		}

		private bool InitP2PClient (bool isHost) {
			try {
				P2PClient.Create (config, isHost);
			} catch (Exception e) {
				MessageBox.Show (string.Format ("Could not initiate P2P node.\n{0}\n\n{1}", e.Message, e.StackTrace.ToString ()));
				return false;
			}
			return true;
		}

		private void ReleaseP2PClient () {
			// Drop reference to p2p client and force GC
			P2PClient.Release ();
			GC.Collect ();
		}

		public void CreateNewGameHandler (object sender, EventArgs e) {
			// Open game creation view.
			if (configGameView == null) {
				ConfigGameState configState = new ConfigGameState(config);
				configGameView = new ConfigureGame (configState, config);
				configGameView.GamePublished += PublishGameHandler;
				configGameView.GameCreateCanceled += CancelConfigureGameHandler;
			}
			DisplayPage (configGameView, null);
		}

		public void JoinGameHandler (object sender, JoinGameEventArgs e) {
			Label errorMsg = e.errorMsg;

			if (!InitP2PClient (false)) {
				return;
			}
			P2Pplayer client = P2PClient.Instance ().client;
			GameLobbyState state = new GameLobbyState(e.game, Config, client);
			InitLobby (client, state);
			GameListState gameList = gameListView.GetState () as GameListState;
			try {
				gameList.JoinGame (e.game, client);
			} catch (Exception ex) {
				errorMsg.Content = string.Format ("Could not join game.\n{0}\n\n{1}", ex.Message, ex.StackTrace.ToString ());
				return;
			}
			DisplayPage (lobbyView, state);
		}

		public void LobbyPeerLeftHandler (object sender, P2PCommEventArgs e) {
			GameLobbyState lobby = lobbyView.GetState () as GameLobbyState;
			lobby.PeerLeftHandler (sender, e);
			if (lobby.IsHostPeer (e.RemoteAddress, e.RemotePort)) {
				// Host left, leave lobby.
				LeaveGameLobbyHandler (sender, e);
			}
		}

		public void StartGameHandler (object sender, GameStartEventArgs e) {
			Application.Current.Dispatcher.BeginInvoke((Action)(() => { StartGameRoutine (); }));
			
			void StartGameRoutine () {
				GameLobbyState lobby = lobbyView.GetState () as GameLobbyState;
				P2Pplayer client = P2PClient.Instance ().client;
				GameState game = new GameState (config, client, lobby.Game, e.bombTime);
				ReleaseLobby (client);
				InitGame (client, game);
				try {
					game.StartGame ();
				} catch (Exception ex) {
					MessageBox.Show (string.Format ("Failed to start game.\n{0}\n\n{1}", ex.Message, ex.StackTrace.ToString ()));
					return;
				}
				DisplayPage (gameView, game);
			}
		}

		public void LeaveGameLobbyHandler (object sender, EventArgs e) {
			Application.Current.Dispatcher.BeginInvoke ((Action)(() => { LeaveGameLobbyRoutine (); }));

			void LeaveGameLobbyRoutine () {
				GameLobbyState lobby = lobbyView.GetState () as GameLobbyState;
				try {
					lobby.LeaveLobby ();
				} catch (Exception ex) {
					MessageBox.Show (string.Format ("Failed to leave game lobby.\n{0}\n\n{1}", ex.Message, ex.StackTrace.ToString ()));
					return;
				}

				ReleaseLobby (P2PClient.Instance ().client);
				ReleaseP2PClient ();
				DisplayPage (gameListView, null);
			}
		}

		public void LeaveGameHandler (object sender, EventArgs e) {
			Application.Current.Dispatcher.BeginInvoke ((Action)(() => { LeaveGameRoutine (); }));
		}

		public void PeerLostGameHandler (object sender, EventArgs e) {
			Application.Current.Dispatcher.BeginInvoke ((Action)(() => { PeerLostGameRoutine (); }));

			void PeerLostGameRoutine () {
				GameState state = gameView.GetState () as GameState;
				if (state == null) {
					return;
				}
				if (state.IsWinner ()) {
					WinGameRoutine ();
					return;
				} else {
					state.ResetBombTimes ();
				}
			}
		}

		public void BombReceivedHandler (object sender, BombReceivedEventArgs e) {
			Application.Current.Dispatcher.BeginInvoke ((Action)(() => { BombReceiveRoutine (); }));

			void BombReceiveRoutine () {
				GameState state = gameView.GetState () as GameState;
				bool success = state.ReceiveBomb (e.bombtime);
				if (!success) {
					MessageBox.Show ("Outdated bomb received.");
				}
				gameView.DoReceiveBomb ();
			}
		}

		public void PassBombHandler (object sender, EventArgs e) {
			Application.Current.Dispatcher.BeginInvoke ((Action)(() => { PassBombRoutine (); }));

			void PassBombRoutine () {
				GameState gameState = gameView.GetState () as GameState;

				PeerInfo? peer = gameState.Client.GetRandomPeer ();
				if (gameState.IsWinner () || !peer.HasValue) {
					WinGameRoutine ();
					return;
				}				
				if (!gameState.HasBomb) {
					return;
				}
				int bombTime = gameState.getRemainingBombTime ();
				if (bombTime <= 0) {
					gameState.Lose ();
					LeaveGameRoutine ();
					MessageBox.Show ("You lost");
					return;
				}
				gameView.DoPassBomb ();
				try {
					gameState.PassBomb (peer.Value, bombTime);
				} catch (Exception ex) {
					MessageBox.Show (string.Format ("Could not pass bomb onwards.\n{0}\n\n{1}", ex.Message, ex.StackTrace.ToString ()));
					gameState.FailPassBomb ();
					gameView.DoFailBombSend ();
					return;
				}
			}
		}

		public void BombSendFailedHandler (object sender, EventArgs e) {
			Application.Current.Dispatcher.BeginInvoke ((Action)(() => { BombSendFailRoutine (); }));

			void BombSendFailRoutine () {
				GameState gameState = gameView.GetState () as GameState;
				gameState.FailPassBomb ();
				gameView.DoFailBombSend ();
			}
		}

		public void WinGameHandler (object sender, EventArgs e) {
			Application.Current.Dispatcher.BeginInvoke ((Action)(() => { WinGameRoutine (); }));
		}

		private void WinGameRoutine () {
			MessageBox.Show ("Victory achieved.");
			LeaveGameRoutine ();
		}

		private void LeaveGameRoutine () {
			GameState game = gameView.GetState () as GameState;
			try {
				PeerInfo? peer = game.Client.GetRandomPeer();
				if (peer.HasValue) {
					game.PassBomb (peer.Value, game.getRemainingBombTime ());
				}
				game.LeaveGame ();
			} catch (Exception ex) {
				MessageBox.Show (string.Format ("Could not leave game.\n{0}\n\n{1}", ex.Message, ex.StackTrace.ToString ()));
				return;
			}
			ReleaseGame (P2PClient.Instance ().client);
			ReleaseP2PClient ();
			DisplayPage (gameListView, null);
		}

		public void QuitHandler (object sender, EventArgs e) {
			// Clean up and resource release if any.
			Application.Current.Shutdown (0);
		}

		public void PublishGameHandler (object sender, PublishGameEventArgs e) {
			if (!InitP2PClient (true)) {
				return;
			}
			GameInfo game;
			try {
				game = e.configState.PublishGame (e.game);
			} catch (Exception ex) {
				MessageBox.Show (string.Format ("Could not register game. Try publishing again.\n{0}\n\n{1}", ex.Message, ex.StackTrace.ToString ()));
				return;
			}
			GameLobbyState state = new GameLobbyState (game, config, P2PClient.Instance ().client);
			InitLobby (P2PClient.Instance ().client, state);
			DisplayPage (lobbyView, state);
		}

		public void CancelConfigureGameHandler (object sender, EventArgs e) {
			DisplayPage (gameListView, null);
		}

		private void DisplayPage (Page page, State state) {
			if (page is IChangePage p) {
				currentView?.Clear ();
				p.Init (state);
				currentView = p;
			} else if (page != null) {
				throw new Exception ("All pages must implement IChangePage.");
			}
			this.DataContext = page;
			page.DataContext = page;
			ContentFrame.Navigate (page);
		}

	}
}
