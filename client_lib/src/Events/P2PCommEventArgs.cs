using System;
using System.Net;
using System.Text.Json;

namespace BombPeliLib.Events
{
    sealed public class P2PCommEventArgs : EventArgs
    {
        readonly public IPEndPoint  peer;
        readonly public JsonElement data;
        
        public P2PCommEventArgs (IPEndPoint peer, JsonElement data) {
            this.peer    = peer;
            this.data    = data;
        }

    }
}
