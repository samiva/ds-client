using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombPeli
{
    class P2PCommEventArgs : EventArgs
    {
        public string RemoteAddress { get; set; }
        public int RemotePort { get; set; }
        public int ID { get; set; }
        public dynamic Data { get; set; }
        public Channel MessageChannel { get; set; } = Channel.DEFAULT;
    }
}
