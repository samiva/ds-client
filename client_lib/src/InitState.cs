using IniParser;
using IniParser.Model; 

namespace BombPeli
{
    class InitState : State
    {
        KeyDataCollection clientConfig;
        public InitState(StateMachine sm) :base(sm)
        {

        }

        public override void BeginState()
        {
            ReadConfig();
        }

        public override void ProcessState()
        {
            throw new System.NotImplementedException();
        }

        public override void EndState()
        {
            throw new System.NotImplementedException();
        }

        void ReadConfig() 
        {
            // Read client configs from file
            var parser = new FileIniDataParser();
            IniData confdata = parser.ReadFile("config.ini");
            clientConfig = confdata["client"];

        }

        void FetchGameList() 
        {
            // Fetch list of games from discovery server
        }
    }
}