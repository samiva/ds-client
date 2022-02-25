using System;
using System.Net;

namespace BombPeliLib.Events
{
	public class PeerLeftEventArgs : EventArgs
	{

		readonly public IPEndPoint peer;
		
		public PeerLeftEventArgs (IPEndPoint peer) {
			this.peer = peer;
		}

	}
}
