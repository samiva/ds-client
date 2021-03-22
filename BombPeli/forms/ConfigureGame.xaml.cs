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
	public partial class ConfigureGame : Page, IChangePage
	{

		public delegate void PublishGameEventHandler (object sender, PublishGameEventArgs e);
		public delegate void CancelCreateGameEventHandler (object sender, EventArgs e);
		public event PublishGameEventHandler GamePublished;
		public event CancelCreateGameEventHandler GameCreateCanceled;

		private Config config;
		private ConfigGameState configState;

		public ConfigureGame (ConfigGameState configState, Config config) {
			InitializeComponent ();
			this.configState = configState;
			this.config = config;
		}

		public void Init (State state) {
		}

		public void Clear () {
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
			GamePublished?.Invoke (this, new PublishGameEventArgs (configState, game, ErrorMsgDisplay));
		}

		private void CancelButton_Click (object sender, RoutedEventArgs e) {
			GameCreateCanceled?.Invoke (this, new EventArgs ());
		}
	}
}
