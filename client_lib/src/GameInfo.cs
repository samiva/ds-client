namespace BombPeli
{
    struct GameInfo
    {
        public GameInfo(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}