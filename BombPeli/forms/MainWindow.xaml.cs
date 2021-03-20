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
		private GameLobby lobby;

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

			gameList = new GameList (games, config);
			gameList.OnCreateGame += CreateNewGameHandler;
			gameList.OnJoinGame += JoinGameHandler;
			gameList.OnQuit += QuitHandler;
			DisplayPage (gameList);
		}

		public void CreateNewGameHandler (object sender, EventArgs e) {
			// Open game creation view.
			if (configGame == null) {
				configGame = new ConfigureGame (config);
				configGame.OnPublishGame += PublishGameHandler;
				configGame.OnCancelCreateGame += CancelConfigureGameHandler;
			}
			DisplayPage (configGame);
		}

		public void JoinGameHandler (object sender, JoinGameEventArgs e) {
			
			bool AbortJoin (GameInfo game, Label errorMsg) {
				try {
					p2pClient.SendQuitGame (game.Ip, game.Port);
				} catch (Exception ex) {
					errorMsg.Content = string.Format ("Failed to cancel join.\n{0}\n\n{1}", ex.Message, ex.StackTrace.ToString ());
					return false;
				}
				return true;
			}
			
			bool SendJoin (GameInfo game, Label errorMsg, in int MAX_RETRIES) {
				bool success = false;
				int i = 0;
				do {
					try {
						p2pClient.SendJoinGame (game.Ip, game.Port);
						success = true;
					} catch (Exception ex) {
						errorMsg.Content = string.Format ("Could not join game. Retrying...\n{0}\n\n{1}", ex.Message, ex.StackTrace.ToString ());
					}
					++i;
				} while (!success && i < MAX_RETRIES);
				return success;
			}

			bool FetchPeers (GameInfo game, Label errorMsg, in int MAX_RETRIES) {
				bool success = false;
				int i = 0;
				do {
					try {
						// TODO: Fetch peer list
						success = true;
					} catch (Exception ex) {
						errorMsg.Content = string.Format ("Could not fetch peer list. Retrying...\n{0}\n\n{1}", ex.Message, ex.StackTrace.ToString ());
					}
					++i;
				} while (!success && i < MAX_RETRIES);
				return success;
			}

			if (!InitP2PClient ()) {
				return;
			}
			const int MAX_RETRIES = 3;
			if (
				!SendJoin (e.game, e.errorMsg, MAX_RETRIES)
				|| !FetchPeers (e.game, e.errorMsg, MAX_RETRIES)
			) {
				AbortJoin (e.game, e.errorMsg);
				return;
			}


			InitLobby ();
			DisplayPage (lobby);
		}

		public void QuitHandler (object sender, EventArgs e) {
			// Clean up and resource release if any.
			Application.Current.Shutdown (0);
		}

		public void PublishGameHandler (object sender, PublishGameEventArgs e) {
			// Create P2P node and post it to service discovery server.
			if (!InitP2PClient ()) {
				return;
			}

			ServiceDiscoveryClient client = new ServiceDiscoveryClient(config);
			try {
				client.RegisterGame (e.game);
			} catch (Exception ex) {
				MessageBox.Show (string.Format("Could not register game. Try publishing again.\n{0}\n\n{1}", ex.Message, ex.StackTrace.ToString ()));
				return;
			}

			InitLobby ();
			DisplayPage (lobby);
		}

		public void CancelConfigureGameHandler (object sender, EventArgs e) {
			DisplayPage (gameList);
		}

		private void DisplayPage (Page page) {
			this.DataContext = page;
			page.DataContext = page;
			ContentFrame.Navigate (page);
		}

		private void InitLobby () {
			if (lobby == null) {
				lobby = new GameLobby ();
				/*
					- P2P client joined
					- P2P client left
				*/
			}
		}

		private bool InitP2PClient () {
			if (p2pClient == null) {
				ushort port = config.GetUshort ("localport");
				P2PComm comm;
				try {
					comm = new P2PComm (port);
				} catch (Exception e) {
					MessageBox.Show (string.Format("Could not initiate P2P node.\n{0}\n\n{1}", e.Message, e.StackTrace.ToString ()));
					return false;
				}
				p2pClient = new P2Pplayer (comm);
			}
			return true;
		}

	}
}
