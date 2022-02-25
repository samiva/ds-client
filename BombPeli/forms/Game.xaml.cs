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
	/// Interaction logic for Game.xaml
	/// </summary>
	public partial class Game : Page, IChangePage
	{
					
		public delegate void PassBombEventHandler (object sender, EventArgs e);
		public delegate void WinGameHandler (object sender, EventArgs e);
		public delegate void LeaveGameEventHandler (object sender, EventArgs e);

		public event PassBombEventHandler?  PassBomb;
		public event LeaveGameEventHandler? LeaveGame;

		private          GameState? gameState;
		readonly private object     guiLock = new object ();
		
		public Game () {
			InitializeComponent ();
		}

		public void Init (State? state) {
			gameState = state as GameState;
			if (gameState?.HasBomb == true) {
				DoReceiveBomb ();
			} else {
				DoPassBomb ();
			}
		}

		public void Clear () {
			P2PApi? client = gameState?.Client;
			if (client == null) {
				return;
			}
			gameState = null;
		}

		public State? GetState () {
			return gameState;
		}

		private void passbomb_Click (object sender, RoutedEventArgs e) {
			PassBomb?.Invoke (this, e);
		}

		private void quit_Click (object sender, RoutedEventArgs e) {
			LeaveGame?.Invoke (this, e);
		}

		public void DoReceiveBomb () {
			lock (this.guiLock) {
				BombImage.Visibility = Visibility.Visible;
				passbomb.IsEnabled = true;
			}
		}

		public void DoFailBombSend () {
			DoReceiveBomb ();
		}

		public void DoPassBomb () {
			lock (this.guiLock) {
				BombImage.Visibility = Visibility.Hidden;
				passbomb.IsEnabled   = false;
			}
		}

	}
}
