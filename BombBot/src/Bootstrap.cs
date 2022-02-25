using System;
using BombPeliLib;

namespace BombBot
{
	public class Bootstrap
	{

		private string  errorStr = string.Empty;
		private Config? config;
		private bool    isHost         = false;
		private int     playerCapacity = 0;
		
		public string getError {
			get { return this.errorStr; }
		}

		public Config getConfig {
			get { return this.config ?? throw new Exception ("Unsuccessful bootstrap."); }
		}

		public bool getIsHost {
			get { return this.isHost; }
		}

		public int getPlayerCapacity {
			get { return this.playerCapacity; }
		}

		public bool init (string[] args, out string configFile) {
			this.parseArgs (args, out configFile);
			try {
				this.config = new Config (configFile);
			} catch (Exception ex) {
				this.errorStr = string.Format ("Failed to load configuration file.\n{0}\n\n{1}", ex.Message, ex.StackTrace);
				return false;
			}
			return true;
		}

		private void parseArgs (string[] args, out string configFile) {
			string host        = string.Empty;
			string playerCount = string.Empty;
			int    length      = args.Length;
			configFile = "config.ini";
			if (length >= 3) {
				configFile  = args [0];
				host        = args [1];
				playerCount = args [2];
			} else if (args.Length >= 1) {
				configFile = args [0];
			}
			this.isHost = host.Equals ("true", StringComparison.InvariantCultureIgnoreCase) || host == "1";
			if (!int.TryParse (playerCount, out this.playerCapacity) || this.playerCapacity < 1) {
				this.playerCapacity = 0;
			}
		}
	}
}
