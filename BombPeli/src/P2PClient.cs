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

		private P2PClient (Config config) {
			ushort port = config.GetUshort ("localport");
			P2PComm comm = new P2PComm (port);
			client = new P2Pplayer (comm);
		}

		~P2PClient () {
			Destroy ();
		}

		static public P2PClient Instance (Config config) {
			return instance ??= new P2PClient (config);
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
