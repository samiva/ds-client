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
			}
			state.StartGame += StartGameHandler;
			state.LeaveLobby += LeaveGameLobbyHandler;
			client.PeerListReceived += lobbyView.PeerListChangedHandler;
			client.PeerJoined += lobbyView.PeerListChangedHandler;
			client.PeerQuit += lobbyView.PeerListChangedHandler;
		}

		private void InitGame (P2Pplayer client) {
			if (gameView == null) {
				gameView = new Game ();
				gameView.LeaveGame += LeaveGameHandler;
				gameView.PassBomb += PassBombHandler;
			}
			client.BombReceived += gameView.BombReceivedHandler;
		}

		private bool InitP2PClient (GameInfo currentGame, bool isHost) {
			try {
				P2PClient.Create (config, isHost, currentGame);
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

			if (!InitP2PClient (e.game, false)) {
				return;
			}
			P2Pplayer client = P2PClient.Instance ().client;
			GameLobbyState state = new GameLobbyState(e.game, Config, client);
			InitLobby (client, state);
			try {
				e.gameList.JoinGame (e.game, client);
			} catch (Exception ex) {
				errorMsg.Content = string.Format ("Could not join game.\n{0}\n\n{1}", ex.Message, ex.StackTrace.ToString ());
				return;
			}
			DisplayPage (lobbyView, state);
		}

		public void StartGameHandler (object sender, StartGameEventArgs e) {
			Application.Current.Dispatcher.BeginInvoke((Action)(() => { StartGameRoutine (); }));
			
			void StartGameRoutine () {
				try {
					e.lobby.DoStartGame ();
				} catch (Exception ex) {
					MessageBox.Show (string.Format ("Failed to start game.\n{0}\n\n{1}", ex.Message, ex.StackTrace.ToString ()));
					return;
				}
				P2Pplayer client = P2PClient.Instance ().client;
				InitGame (client);
				DisplayPage (gameView, new GameState (config, client));
			}
		}

		public void LeaveGameLobbyHandler (object sender, LeaveGameLobbyEventArgs e) {
			try {
				e.lobby.DoLeaveLobby ();
			} catch (Exception ex) {
				MessageBox.Show (string.Format ("Failed to leave game lobby.\n{0}\n\n{1}", ex.Message, ex.StackTrace.ToString ()));
				return;
			}
			lobbyView.Clear ();
			ReleaseP2PClient ();
			DisplayPage (gameListView, null);
		}

		public void PassBombHandler (object sender, PassBombEventArgs e) {
			try {
				e.game.PassBomb ();
			} catch (Exception ex) {
				MessageBox.Show (string.Format ("Could not pass bomb onwards.\n{0}\n\n{1}", ex.Message, ex.StackTrace.ToString ()));
				e.game.FailPassBomb ();
				return;
			}
		}

		public void LeaveGameHandler (object sender, LeaveGameEventArgs e) {
			try {
				e.game.LeaveGame ();
			} catch (Exception ex) {
				MessageBox.Show (string.Format ("Could not leave game.\n{0}\n\n{1}", ex.Message, ex.StackTrace.ToString ()));
				return;
			}
			ReleaseP2PClient ();
			DisplayPage (gameListView, null);
		}

		public void QuitHandler (object sender, EventArgs e) {
			// Clean up and resource release if any.
			Application.Current.Shutdown (0);
		}

		public void PublishGameHandler (object sender, PublishGameEventArgs e) {
			if (!InitP2PClient (e.game, true)) {
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
