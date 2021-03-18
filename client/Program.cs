using System;
using System.Collections.Generic;

using BombPeli;

namespace client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Config config = new Config("config.ini");
            ServiceDiscoveryClient client = new ServiceDiscoveryClient(config);
            List<GameInfo> games = client.FetchGameList ();

        }
    }
}
