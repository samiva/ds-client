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
	/// Interaction logic for ConfigureGame.xaml
	/// </summary>
	public partial class ConfigureGame : Page
	{

		public delegate void PublishGameEventHandler (object sender, PublishGameEventArgs e);
		public delegate void CancelCreateGameEventHandler (object sender, EventArgs e);
		public event PublishGameEventHandler OnPublishGame;
		public event CancelCreateGameEventHandler OnCancelCreateGame;

		private Config config;

		public ConfigureGame (Config config) {
			InitializeComponent ();
			this.config = config;
		}

		private void PublishButton_Click (object sender, RoutedEventArgs e) {
			StringBuilder text = new StringBuilder (GameName.Text);
			string gameName = text.ToString();
			if (gameName.Length < 4) {
				MessageBox.Show ("Name must be at least 4 characters long.");
				return;
			}
			if (gameName.Length > 30) {
				MessageBox.Show ("Name must be less than 30 characters long.");
				return;
			}
			GameInfo game = ServiceDiscoveryClient.CreateNewGameInstance(gameName, config.GetUshort("localport"));
			OnPublishGame?.Invoke (this, new PublishGameEventArgs (game));
		}

		private void CancelButton_Click (object sender, RoutedEventArgs e) {
			OnCancelCreateGame?.Invoke (this, new EventArgs ());
		}
	}
}
