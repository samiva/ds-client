using System;

namespace BombPeliLib
{
    class ConfigGameState : State 
    {

        private Config config;
        private P2PComm p2p;

        public ConfigGameState(StateMachine sm, Config config, P2PComm p2p): base(sm)
        {
            this.config = config;
            this.p2p = p2p;
        }

        public override void BeginState()
        {
        }

        public override void ProcessState()
        {
            PublishGame();
        }

        public override void EndState()
        {
        }

        private void PublishGame() 
        {

            GameInfo gi = AskGameInfo();
            // Game info to discovery server
            stateMachine.ChangeState(new GameLobbyState(stateMachine, gi, config, p2p, Role.HOST));
       
        }

        private GameInfo AskGameInfo()
        {
            Console.Write("Input game name: ");
            var gameName=Console.ReadLine();
            Console.WriteLine("the name of new game {0}", gameName);
            return ServiceDiscoveryClient.CreateNewGameInstance (gameName, config.GetUshort ("localport"));
        }
    }
}