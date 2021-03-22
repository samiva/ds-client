using System;
using System.Collections.Generic;

namespace BombPeliLib
{
    public struct PeerInfo
    {
        public string Address { get; private set; }
        public int Port { get; private set; }

        public PeerInfo(string address, int port) {
            Address = address;
            Port = port;
        }
    }

	public class PeerInfoComparer : IEqualityComparer<PeerInfo>
	{
        static public PeerInfoComparer instance = new PeerInfoComparer();

        private PeerInfoComparer () {
        }

		public bool Equals (PeerInfo a, PeerInfo b) {
            return
                a.Address == b.Address
                && a.Port == b.Port;
		}

		public int GetHashCode (PeerInfo obj) {
            return HashCode.Combine<string, int> (obj.Address, obj.Port);
		}
	}
}
