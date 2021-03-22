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
					
		public delegate void PassBombEventHandler (object sender, PassBombEventArgs e);
		public delegate void LeaveGameEventHandler (object sender, LeaveGameEventArgs e);
		public event PassBombEventHandler PassBomb;
		public event LeaveGameEventHandler LeaveGame;

		private GameState gameState;

		public Game () {
			InitializeComponent ();
		}

		public void Init (State state) {
			this.gameState = state as GameState;
		}

		public void Clear () {
			gameState = null;
		}

		private void passbomb_Click (object sender, RoutedEventArgs e) {
			BombImage.Visibility = Visibility.Hidden;
			PassBomb?.Invoke (this, new PassBombEventArgs (gameState));
		}

		private void quit_Click (object sender, RoutedEventArgs e) {
			LeaveGame?.Invoke (this, new LeaveGameEventArgs (gameState));
		}

		public void BombReceivedHandler (object sender, EventArgs e) {
			Application.Current.Dispatcher.BeginInvoke ((Action)(() => { DoBombReceivedHandler (); }));
		}

		public void DoBombReceivedHandler () {
			BombImage.Visibility = Visibility.Visible;
		}
	}
}
