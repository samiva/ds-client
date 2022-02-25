using System;
using System.Net;

namespace BombPeliLib.Events
{
	public class PeerJoinedEventArgs : EventArgs
	{
		
		readonly public IPEndPoint peer;
		
		public PeerJoinedEventArgs (IPEndPoint peer) {
			this.peer = peer;
		}
		
	}
}
