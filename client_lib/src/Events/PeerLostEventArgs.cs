using System;
using System.Net;

namespace BombPeliLib.Events
{
	public class PeerLostEventArgs : EventArgs
	{
		
		readonly public IPEndPoint peer;
		
		public PeerLostEventArgs (IPEndPoint peer) {
			this.peer = peer;
		}
		
	}
}
