using System;
using kevincastejon;

namespace BombPeli
{
    class ConfigGameState : State 
    {

        private Config config;

        public ConfigGameState(StateMachine sm, Config config): base(sm)
        {
            this.config = config;
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
            stateMachine.ChangeState(new GameLobbyState(stateMachine, gi, config));
       
        }

        private GameInfo AskGameInfo()
        {
            Console.Write("Input game name: ");
            var gameName=Console.ReadLine();
            Console.WriteLine("the name of new game {0}", gameName);
            return new GameInfo(gameName);
        }
    }
}