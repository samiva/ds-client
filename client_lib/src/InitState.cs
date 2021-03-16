using IniParser;
using IniParser.Model;

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BombPeli
{
    class InitState : State
    {

        private Config config;
        private string configPath;
        

        public InitState(StateMachine sm, string configPath) :base(sm)
        {
            this.configPath = configPath;
        }

        public override void BeginState()
        {
            ReadConfig ();
            FetchGameList ();
        }

        public override void ProcessState()
        {
            throw new System.NotImplementedException();
        }

        public override void EndState()
        {
            throw new System.NotImplementedException();
        }

        private void ReadConfig () {
            config = new Config (configPath);
        }

        private void FetchGameList() 
        {
            ServiceDiscoveryClient client = new ServiceDiscoveryClient (config);
            List<GameInfo> games = client.FetchGameList();
        }
    }
}