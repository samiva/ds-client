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
			GameInfo game = e.game;
			// TODO: Join P2P network and open lobby
			InitLobby ();
			DisplayPage (lobby);
		}

		public void QuitHandler (object sender, EventArgs e) {
			// Clean up and resource release if any.
			Application.Current.Shutdown (0);
		}

		public void PublishGameHandler (object sender, PublishGameEventArgs e) {
			// Create P2P network and post it to service discovery server.
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
	}
}
