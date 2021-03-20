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
			initView.OnInitComplete += InitCompleteHandler;
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
			gameListView.OnCreateGame += CreateNewGameHandler;
			gameListView.OnJoinGame += JoinGameHandler;
			gameListView.OnQuit += QuitHandler;
			DisplayPage (gameListView, null);
		}

		private void InitLobby () {
			if (lobbyView == null) {
				lobbyView = new GameLobby ();
				lobbyView.OnStartGame += StartGameHandler;
				lobbyView.OnLeaveGame += LeaveGameLobbyHandler;
				/*
					- P2P client joined
					- P2P client left
				*/
			}
		}

		private void InitGame () {
			if (gameView == null) {
				gameView = new Game ();
				gameView.OnLeaveGame += LeaveGameHandler;
				gameView.OnPassBomb += PassBombHandler;
			}
			gameView.Init (new GameState (P2PClient.Instance (config).client));
		}

		private bool InitP2PClient () {
			try {
				P2PClient.Instance (config);
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
				configGameView.OnPublishGame += PublishGameHandler;
				configGameView.OnCancelCreateGame += CancelConfigureGameHandler;
			}
			DisplayPage (configGameView, null);
		}

		public void JoinGameHandler (object sender, JoinGameEventArgs e) {
			Label errorMsg = e.errorMsg;

			if (!InitP2PClient ()) {
				return;
			}
			List<PeerInfo> peers;
			try {
				peers = e.gameList.JoinGame (e.game, P2PClient.Instance (config).client);
			} catch (Exception ex) {
				errorMsg.Content = string.Format ("Could not join game.\n{0}\n\n{1}", ex.Message, ex.StackTrace.ToString ());
				return;
			}
			if (peers == null) {
				return;
			}
			InitLobby ();
			DisplayPage (lobbyView, new GameLobbyState (e.game, peers, config, P2PClient.Instance (config).client, false));
		}

		public void StartGameHandler (object sender, StartGameEventArgs e) {
			try {
				e.lobby.StartGame ();
			} catch (Exception ex) {
				MessageBox.Show (string.Format ("Failed to start game.\n{0}\n\n{1}", ex.Message, ex.StackTrace.ToString ()));
				return;
			}

			InitGame ();
		}

		public void LeaveGameLobbyHandler (object sender, LeaveGameLobbyEventArgs e) {
			try {
				e.lobby.LeaveLobby ();
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
			gameView.Clear ();
			ReleaseP2PClient ();
			DisplayPage (gameListView, null);
		}

		public void QuitHandler (object sender, EventArgs e) {
			// Clean up and resource release if any.
			Application.Current.Shutdown (0);
		}

		public void PublishGameHandler (object sender, PublishGameEventArgs e) {
			if (!InitP2PClient ()) {
				return;
			}
			GameInfo game;
			try {
				game = e.configState.PublishGame (e.game);
			} catch (Exception ex) {
				MessageBox.Show (string.Format ("Could not register game. Try publishing again.\n{0}\n\n{1}", ex.Message, ex.StackTrace.ToString ()));
				return;
			}
			InitLobby ();
			DisplayPage (lobbyView, new GameLobbyState (game, new List<PeerInfo>(), config, P2PClient.Instance (config).client, true));
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
