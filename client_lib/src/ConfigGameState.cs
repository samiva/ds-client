using System;
using kevincastejon;

namespace BombPeli
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
            throw new NotImplementedException();
        }

        public override void ProcessState()
        {
            PublishGame();
        }

        public override void EndState()
        {
            throw new NotImplementedException();
        }

        private void PublishGame() 
        {

            GameInfo gi = AskGameInfo();
            // Game info to discovery server
            stateMachine.ChangeState(new GameLobbyState(stateMachine, gi, config, p2p));
       
        }

        private GameInfo AskGameInfo()
        {
            Console.Write("Input game name: ");
            var gameName=Console.ReadLine();
            Console.WriteLine("the name of new game {0}", gameName);
            
            return new GameInfo(0, gameName, "", config.GetUshort ("localport"), GameStatus.OPEN);
        }
    }
}