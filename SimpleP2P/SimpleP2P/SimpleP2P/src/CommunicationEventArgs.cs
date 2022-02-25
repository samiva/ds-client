using System;
using System.Net;

namespace SimpleP2P
{
	sealed internal class CommunicationEventArgs : EventArgs
	{

		readonly public IPEndPoint peer;
		readonly public byte[]     msg;

		public CommunicationEventArgs (IPEndPoint peer, byte[] msg) {
			this.peer = peer;
			this.msg  = msg;
		}

	}
}
