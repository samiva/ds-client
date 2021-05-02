
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombPeliLib
{
    public class P2PCommEventArgs : EventArgs
    {
        public string RemoteAddress { get; set; }
        public int RemotePort { get; set; }
        public int ID { get; set; }
        public dynamic Data { get; set; }
        public Channel MessageChannel { get; set; } = Channel.DEFAULT;

        public P2PCommEventArgs () {
        }

        public P2PCommEventArgs (
            string remoteAddress,
            int remotePort,
            int id,
            dynamic data,
            Channel channel
        ) {
            RemoteAddress = remoteAddress;
            RemotePort = remotePort;
            ID = id;
            Data = data;
            MessageChannel = channel;
        }

        //static public explicit operator P2PCommEventArgs (UDPManagerEvent e) {
        //    Channel channel;
        //    if (!Enum.TryParse<Channel> (e.UDPdataInfo.ChannelName, true, out channel)) {
        //        channel = Channel.UNKNOWN;
        //    }

        //    return new P2PCommEventArgs (
        //        e.UDPdataInfo.RemoteAddress,
        //        e.UDPdataInfo.RemotePort,
        //        e.UDPdataInfo.ID,
        //        e.UDPdataInfo.Data,
        //        channel
        //    );
        //}

    }
}
