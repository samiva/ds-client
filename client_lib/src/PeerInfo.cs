using System;
using System.Collections.Generic;
using System.Net;

namespace BombPeliLib
{
    public struct PeerInfo
	{

		readonly public IPEndPoint ip;

		public PeerInfo (IPEndPoint ip) {
			this.ip = ip;
		}

		public PeerInfo(string address, int port) {
			this.ip = new IPEndPoint (IPAddress.Parse (address), port);
		}
    }

	public class PeerInfoComparer : IEqualityComparer<PeerInfo>
	{
        static readonly public PeerInfoComparer instance = new PeerInfoComparer();

        private PeerInfoComparer () {
        }

		public bool Equals (PeerInfo a, PeerInfo b) {
            return a.ip.Equals (b.ip);
		}

		public int GetHashCode (PeerInfo obj) {
			return obj.ip.GetHashCode ();
		}
	}
}
