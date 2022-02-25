using System.Net;

namespace SimpleP2P
{
	internal enum PeerStatus : byte {
		OK = 0x00,
		DYING = 0x01,
		DEAD = 0x02,
	}

	internal struct PeerEntry
	{

		readonly public IPEndPoint peer;
		public          long       lastMsg;
		public          PeerStatus status;
		
		public PeerEntry (IPEndPoint peer) {
			this.peer    = peer;
			this.lastMsg = 0;
			this.status  = PeerStatus.OK;
		}
	}
}
