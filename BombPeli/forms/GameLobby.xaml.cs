using BombPeliLib;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BombPeliLib.Events;

namespace BombPeli
{
    /// <summary>
    /// Interaction logic for GameLobby.xaml
    /// </summary>
    public partial class GameLobby : Page, IChangePage
    {

        public delegate void StartGameEventHandler(object   sender, GameStartEventArgs e);
        public delegate void LeaveLobbyEventHandler (object sender, EventArgs          e);
        
        public event StartGameEventHandler?  StartGame;
        public event LeaveLobbyEventHandler? LeaveLobby;

        private GameLobbyState?                    lobby;
        private ObservableCollection<PeerInfoView> peersView = new ObservableCollection<PeerInfoView> ();

        public GameLobby() {
            InitializeComponent();
        }

        public void Init(State? state) {
            this.lobby = state as GameLobbyState;
            this.updatePeerList ();
            this.ListBoxPeers.DataContext = this;
            this.ViewControls.Children.Clear();
            if (lobby?.IsHost == true) {
                ViewControls.Children.Add(MakeStartButton());
            }
            ViewControls.Children.Add (MakeQuitButton ());
        }

        public void Clear() {
            lobby = null;
        }

        public State? GetState() {
            return lobby;
        }

        public ObservableCollection<PeerInfoView> PeersView {
            get {
                return this.peersView;
            }
        }

        private Button MakeStartButton() {
            Button button = new Button ();
            button.Name      =  "start";
            button.Content   =  "Start game";
            button.Margin    =  new Thickness(10);
            button.Height    =  30;
            button.IsEnabled =  false;
            button.Click     += start_Click;
            return button;
        }

        private Button MakeQuitButton() {
            Button button = new Button();
            button.Name    =  "quit";
            button.Content =  "Leave game";
            button.Margin  =  new Thickness(10);
            button.Height  =  30;
            button.Click   += quit_Click;
            return button;
        }

        private void start_Click(object sender, RoutedEventArgs e) {
            StartGame?.Invoke(this, new GameStartEventArgs(lobby.BombTime));
        }

        private void quit_Click(object sender, RoutedEventArgs e) {
            LeaveLobby?.Invoke(this, e);
        }

        public void PeerListChangedHandler(object sender, EventArgs e) {
            Application.Current.Dispatcher.BeginInvoke((Action)(() => { DoPeerListHandler(); }));
        }

        public void DoPeerListHandler() {
            this.updatePeerList ();
            if (lobby.IsHost) {
                enableStartButton(!ListBoxPeers.Items.IsEmpty);
            }
        }

        private void enableStartButton(bool enable) {
            Button btn = ViewControls.Children.OfType<Button>().Single(child => child.Name == "start");
            btn.IsEnabled = enable; 
        }

        private void updatePeerList () {
            List<PeerInfo> currentPeers = this.lobby?.Peers ?? new List<PeerInfo> ();
            int            peerCount    = currentPeers.Count;
            int            viewCount    = this.peersView.Count;
            for (int i = peerCount; i < viewCount; ++i) {
                this.peersView.RemoveAt (peerCount);
            }
            for (int i = viewCount; i < peerCount; ++i) {
                this.peersView.Add (new PeerInfoView ());
            }
            for (int i = 0; i < peerCount; ++i) {
                this.peersView [i].Peer = currentPeers [i].ip.Address.ToString ();
                this.peersView [i].Port = (ushort)currentPeers [i].ip.Port;
            }
        }
    }
}
