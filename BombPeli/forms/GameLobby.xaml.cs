using BombPeliLib;

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

namespace BombPeli
{
	/// <summary>
	/// Interaction logic for GameLobby.xaml
	/// </summary>
	public partial class GameLobby : Page, IChangePage
	{

		public delegate void StartGameEventHandler (object sender, StartGameEventArgs e);
		public delegate void LeaveGameEventHandler (object sender, LeaveGameLobbyEventArgs e);
		public event StartGameEventHandler OnStartGame;
		public event LeaveGameEventHandler OnLeaveGame;

		private GameLobbyState lobby;

		public GameLobby () {
			InitializeComponent ();
		}

		public void Init (State state) {
			this.lobby = state as GameLobbyState;

			ListBoxPeers.DataContext = this;
			ListBoxPeers.ItemsSource = Peers;
			ViewControls.Children.Clear ();
			if (lobby.IsHost) {
				ViewControls.Children.Add (MakeStartButton ());
			}
			ViewControls.Children.Add (MakeQuitButton ());
		}

		public void Clear () {
			lobby = null;
		}

		public List<PeerInfo> Peers {
			get { return lobby.Peers; }
		}

		private Button MakeStartButton () {
			Button button = new Button ();
			button.Name = "start";
			button.Click += start_Click;
			button.Content = "Start game";
			return button;
		}

		private Button MakeQuitButton () {
			Button button = new Button ();
			button.Name = "quit";
			button.Click += quit_Click;
			button.Content = "Leave game";
			return button;
		}

		private void start_Click (object sender, RoutedEventArgs e) {
			OnStartGame?.Invoke (this, new StartGameEventArgs (lobby));
		}

		private void quit_Click (object sender, RoutedEventArgs e) {
			OnLeaveGame?.Invoke (this, new LeaveGameLobbyEventArgs (lobby));
		}

	}
}
