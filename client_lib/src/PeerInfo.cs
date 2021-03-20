namespace BombPeliLib
{
    struct PeerInfo
    {
        public string Address { get; private set; }
        public int Port { get; private set; }
        public PeerInfo(string address, int port)
        {
            Address = address;
            Port = port;
        }
    }
}
