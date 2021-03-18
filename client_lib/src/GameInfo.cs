namespace BombPeli
{
    struct GameInfo {

        public ulong Id {
            get; private set;
        }

        public string Name {
            get; private set;
        }

        public string Ip {
            get; private set;
        }

        public ushort Port {
            get; private set;
        }

        public GameStatus Status {
            get; private set;
        }

        public GameInfo (ulong id, string name, string ip, ushort port, GameStatus status) {
            Id = id;
            Name = name;
            Ip = ip;
            Port = port;
            Status = status;
        }

        
    }
}