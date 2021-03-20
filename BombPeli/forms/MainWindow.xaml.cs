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

		private Init initView;
		private GameList gameList;
		private ConfigureGame configGame;
		private GameLobby lobbyView;
		private P2Pplayer p2pClient;

		public MainWindow () {
			InitializeComponent ();

			initView = new Init ();
			initView.OnInitComplete += InitCompleteHandler;
			ContentFrame.Navigate (initView);
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
			gameList = new GameList (gameListState, config);
			gameList.OnCreateGame += CreateNewGameHandler;
			gameList.OnJoinGame += JoinGameHandler;
			gameList.OnQuit += QuitHandler;
			DisplayPage (gameList);
		}

		private void InitLobby (P2Pplayer client, GameInfo game, List<PeerInfo> peers, bool isHost) {
			if (lobbyView == null) {
				lobbyView = new GameLobby ();
				lobbyView.OnStartGame += StartGameHandler;
				lobbyView.OnLeaveGame += LeaveGameHandler;
				/*
					- P2P client joined
					- P2P client left
				*/
			}
			lobbyView.Init (new GameLobbyState (game, peers, config, client, isHost));
		}

		private bool InitP2PClient () {
			ushort port = config.GetUshort ("localport");
			P2PComm comm;
			try {
				comm = new P2PComm (port);
			} catch (Exception e) {
				MessageBox.Show (string.Format ("Could not initiate P2P node.\n{0}\n\n{1}", e.Message, e.StackTrace.ToString ()));
				return false;
			}
			p2pClient = new P2Pplayer (comm);
			return true;
		}

		public void CreateNewGameHandler (object sender, EventArgs e) {
			// Open game creation view.
			if (configGame == null) {
				ConfigGameState configState = new ConfigGameState(config);
				configGame = new ConfigureGame (configState, config);
				configGame.OnPublishGame += PublishGameHandler;
				configGame.OnCancelCreateGame += CancelConfigureGameHandler;
			}
			DisplayPage (configGame);
		}

		public void JoinGameHandler (object sender, JoinGameEventArgs e) {
			Label errorMsg = e.errorMsg;

			if (!InitP2PClient ()) {
				return;
			}
			List<PeerInfo> peers;
			try {
				peers = e.gameList.JoinGame (e.game, p2pClient);
			} catch (Exception ex) {
				errorMsg.Content = string.Format ("Could not join game.\n{0}\n\n{1}", ex.Message, ex.StackTrace.ToString ());
				return;
			}
			if (peers == null) {
				return;
			}
			InitLobby (p2pClient, e.game, peers, false);
			DisplayPage (lobbyView);
		}

		public void StartGameHandler (object sender, StartGameEventArgs e) {
			try {
				e.lobby.StartGame ();
			} catch (Exception ex) {
				MessageBox.Show (string.Format ("Failed to start game.\n{0}\n\n{1}", ex.Message, ex.StackTrace.ToString ()));
				return;
			}
		}

		public void LeaveGameHandler (object sender, LeaveGameEventArgs e) {
			try {
				e.lobby.LeaveGame ();
			} catch (Exception ex) {
				MessageBox.Show (string.Format ("Failed to leave game lobby.\n{0}n\n{1}", ex.Message, ex.StackTrace.ToString ()));
				return;
			}
			
			DisplayPage (gameList);
		}

		public void QuitHandler (object sender, EventArgs e) {
			// Clean up and resource release if any.
			Application.Current.Shutdown (0);
		}

		public void PublishGameHandler (object sender, PublishGameEventArgs e) {
			if (!InitP2PClient ()) {
				return;
			}
			try {
				e.configState.PublishGame (e.game);
			} catch (Exception ex) {
				MessageBox.Show (string.Format ("Could not register game. Try publishing again.\n{0}\n\n{1}", ex.Message, ex.StackTrace.ToString ()));
				return;
			}
			InitLobby (p2pClient, e.game, new List<PeerInfo> (), true);
			DisplayPage (lobbyView);
		}

		public void CancelConfigureGameHandler (object sender, EventArgs e) {
			DisplayPage (gameList);
		}

		private void DisplayPage (Page page) {
			this.DataContext = page;
			page.DataContext = page;
			ContentFrame.Navigate (page);
		}

	}
}
