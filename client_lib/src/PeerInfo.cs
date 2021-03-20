using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kevincastejon;

namespace BombPeliLib
{
    struct PeerInfo
    {
        public string Address { get; private set; }
        public int Port { get; private set; }
        PeerInfo(string address, int port)
        {
            Address = address;
            Port = port;
        }
    }
}
