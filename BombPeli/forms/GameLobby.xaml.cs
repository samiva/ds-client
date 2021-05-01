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

        public delegate void StartGameEventHandler(object sender, GameStartEventArgs e);
        public delegate void LeaveLobbyEventHandler(object sender, EventArgs e);
        public event StartGameEventHandler StartGame;
        public event LeaveLobbyEventHandler LeaveLobby;

        private GameLobbyState lobby;

        public GameLobby()
        {
            InitializeComponent();
        }

        public void Init(State state)
        {
            this.lobby = state as GameLobbyState;

            ListBoxPeers.DataContext = this;
            ListBoxPeers.ItemsSource = Peers;
            ViewControls.Children.Clear();
            if (lobby.IsHost)
            {
                ViewControls.Children.Add(MakeStartButton());
            }
            ViewControls.Children.Add(MakeQuitButton());
        }

        public void Clear()
        {
            lobby = null;
        }

        public State GetState()
        {
            return lobby;
        }

        public List<PeerInfo> Peers
        {
            get { return lobby.Peers; }
        }

        private Button MakeStartButton()
        {
            Button button = new Button();
            button.Name = "start";
            button.Click += start_Click;
            button.Content = "Start game";
            button.Margin = new Thickness(10);
            button.Height = 30;
            button.IsEnabled = false;
            return button;
        }

        private Button MakeQuitButton()
        {
            Button button = new Button();
            button.Name = "quit";
            button.Click += quit_Click;
            button.Content = "Leave game";
            button.Margin = new Thickness(10);
            button.Height = 30;
            return button;
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            StartGame?.Invoke(this, new GameStartEventArgs(lobby.BombTime));
        }

        private void quit_Click(object sender, RoutedEventArgs e)
        {
            LeaveLobby?.Invoke(this, e);
        }

        public void PeerListChangedHandler(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() => { DoPeerListHandler(); }));
        }

        public void DoPeerListHandler()
        {
            ListBoxPeers.Items.Refresh();
            if (lobby.IsHost)
            {
                if (!ListBoxPeers.Items.IsEmpty)
                {
                    enableStartButton(true);
                }
                else
                {
                    enableStartButton(false);
                }
            }
        }

        private void enableStartButton(bool enable)
        {
            
                var btn = ViewControls.Children.OfType<Button>().Single(child => child.Name == "start");
                btn.IsEnabled = enable;
            
        }

    }
}
