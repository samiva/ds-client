using BombPeliLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombPeli
{
	public class P2PClient
	{

		static private P2PClient instance;
		public P2Pplayer client;

		private P2PClient (Config config, bool isHost, GameInfo currentGame) {
			ushort port = config.GetUshort ("localport");
			P2PComm comm = new P2PComm (port);
			client = new P2Pplayer (comm, isHost, currentGame);
		}

		~P2PClient () {
			Destroy ();
		}

		static public void Create (Config config, bool isHost, GameInfo currentGame) {
			Release ();
			instance = new P2PClient (config, isHost, currentGame);
		}

		static public P2PClient Instance () {
			return instance ?? throw new Exception ("No P2P client instance.");
		}

		static public void Release () {
			if (instance == null) {
				return;
			}
			instance.Destroy ();
			instance = null;
		}

		private void Destroy () {
			client?.Close ();
			client = null;
		}
	}
}
