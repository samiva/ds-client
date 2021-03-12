using System.Collections.Generic;

namespace BombPeli
{
    class Client {
        private Dictionary<string,string> config;
        public void Init() {
            ReadConfig();
        } 

        private void ReadConfig() {
            config = new Dictionary<string, string>();
        }

        public List<GameInfo> FetchGameList() {
            return new List<GameInfo>();
        }
    }
}