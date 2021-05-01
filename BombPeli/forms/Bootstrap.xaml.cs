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
	/// Interaction logic for Init.xaml
	/// </summary>
	public partial class Bootstrap : Page, IChangePage
	{

		public delegate void InitCompleteHandler (object sender, EventArgs e);
		public event InitCompleteHandler InitComplete;

		public Bootstrap () {
			InitializeComponent ();
		}

		public Config Config {
			get; private set;
		}

		public List<GameInfo> Games {
			get; private set;
		}

		public void Init (State state) {
		}

		public void Clear () {
		}

		public State GetState () {
			return null;
		}

		private void Page_Loaded (object sender, RoutedEventArgs e) {
			string [] args = Environment.GetCommandLineArgs ();
			string configFile = "config.ini";
			if (args.Length > 1) {
				configFile = args[1];
			}
			try {
				Config = new Config (configFile);
			} catch (Exception ex) {
				MessageBox.Show (string.Format ("Failed to load configuration file.\n{0}\n\n{1}", ex.Message, ex.StackTrace.ToString ()));
				Application.Current.Shutdown (-1);
				return;
			}

			ServiceDiscoveryClient client = new ServiceDiscoveryClient(Config);
			try {
				Games = client.FetchGameList ();
			} catch (Exception ex) {
				MessageBox.Show (string.Format ("Failed to fetch game list.\n{0}\n\n{1}", ex.Message, ex.StackTrace.ToString ()));
				Application.Current.Shutdown (-1);
				return;
			}

			InitComplete?.Invoke (this, new EventArgs ());
		}

	}

}
