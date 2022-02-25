using System;
using System.Net;

namespace SimpleP2P
{
	sealed public class ConnectionEventArgs : EventArgs
	{

		readonly public long       id;
		readonly public IPEndPoint peer;
		readonly public byte[]     data;
		public ConnectionEventArgs (long id, IPEndPoint peer, byte[]? data = null) {
			this.id   = id;
			this.peer = peer;
			this.data = data ?? Array.Empty<byte> ();
		}

	}
}
