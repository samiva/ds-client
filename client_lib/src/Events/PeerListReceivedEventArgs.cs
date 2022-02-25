using System;
using System.Net;

namespace BombPeliLib.Events
{
	public class PeerListReceivedEventArgs : EventArgs
	{

		readonly public IPEndPoint[] peers;

		public PeerListReceivedEventArgs (IPEndPoint[] peers) {
			this.peers = peers;
		}

	}
}
