using System.Net;

namespace BombPeliLib
{
    public struct GameInfo {

        public  ulong       id       { get; set; }
        public  string      name     { get; set; }
        public  string      ip       { get; set; }
        public  ushort      port     { get; set; }
        public  GameStatus  status   { get; set; }
        private IPEndPoint? endpoint { get; set; }

        public GameInfo (ulong id, string name, string ip, ushort port, GameStatus status) {
            this.id       = id;
            this.name     = name;
            this.ip       = ip;
            this.port     = port;
            this.status   = status;
            this.endpoint = null;
        }

        public IPEndPoint getEndpoint () {
            return this.endpoint ??= new IPEndPoint (IPAddress.Parse (this.ip), this.port);
        }

    }
}