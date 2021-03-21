using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for GameList.xaml
    /// </summary>
    public partial class GameList : Page, IChangePage
    {

        public delegate void GameListEventHandler (object sender, EventArgs e);
        public delegate void JoinGameEventHandler (object sender, JoinGameEventArgs e);
        public event GameListEventHandler OnCreateGame;
        public event JoinGameEventHandler OnJoinGame;
        public event GameListEventHandler OnQuit;

        private GameListState gameList;
        private Config config;
        private ObservableCollection<GameInfoView> gameViews = new ObservableCollection<GameInfoView> ();

        public GameList (GameListState gameList, Config config) {
		    InitializeComponent ();
            this.gameList = gameList;
            this.config = config;
            MakeViews ();
            ListBoxGames.DataContext = this;
        }

        public ObservableCollection<GameInfoView> Games {
            get {
                return gameViews;
            }
        }

        public void Init (State state) {
        }

        public void Clear () {
            
        }

        private void MakeViews () {
            /*
                MEMO: Likely awful performance for more than a few items long collections.
                Reason being the large number of events that will be fired upon execution.
            */
            List<GameInfo> games = gameList.Games;
            games.Add (new GameInfo (123, "asd", "12341234", 1234, GameStatus.ENDED));
            int gameCount = games.Count;
            int viewCount = gameViews.Count;

            for (
                int i = gameCount, count = viewCount;
                i < count;
                ++i
            ) {
                gameViews.RemoveAt (i);
            }
            for (
                int i = viewCount, count = gameCount;
                i < count;
                ++i
            ) {
                gameViews.Add (new GameInfoView ());
            }
            
            for (int i = 0; i < gameCount; ++i) {
                gameViews[i].Name = games[i].Name;
                gameViews[i].Port = games[i].Port;
            }
        }

		private void newgame_Click (object sender, RoutedEventArgs e) {
            OnCreateGame?.Invoke (this, e);
		}

		private void joingame_Click (object sender, RoutedEventArgs e) {
            int index = ListBoxGames.SelectedIndex;
            if (index == -1) {
                MessageBox.Show ("Select game to join from the list.");
                return;
            }
            if (gameList.Games.Count > index) {
                OnJoinGame?.Invoke (this, new JoinGameEventArgs (gameList, gameList.Games [index], ErrorMsgDisplay));
            }
		}

		private void refresh_Click (object sender, RoutedEventArgs e) {
            ServiceDiscoveryClient client = new ServiceDiscoveryClient(config);
            List<GameInfo> games;
            try {
                games = client.FetchGameList ();
            } catch (Exception ex) {
                MessageBox.Show (string.Format ("Failed to refresh game list.\n{0}\n\n{1}", ex.Message, ex.StackTrace.ToString ()));
                return;
            }
            gameList.Games = games;
            MakeViews ();
		}

		private void quit_Click (object sender, RoutedEventArgs e) {
            OnQuit?.Invoke (this, e);
		}

        private void fetchGames () {
            ServiceDiscoveryClient client = new ServiceDiscoveryClient(config);
        }
	}
}
