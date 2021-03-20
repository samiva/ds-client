using System;
using System.Collections.Generic;

namespace BombPeliLib
{
    
    class Game {
        public static Game Create() {
            return new Game();
        }  

        public void Config(Dictionary<string,string> conf) {
            Console.WriteLine("configurate game");
        }
    }
}