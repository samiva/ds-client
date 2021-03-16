using System;
using kevincastejon;

namespace BombPeli
{
    class ConfigGameState : State 
    {


        public ConfigGameState(StateMachine sm): base(sm)
        {
        }

        public override void BeginState()
        {
            throw new System.NotImplementedException();
        }

        public override void ProcessState()
        {
            PublishGame();  
        }

        public override void EndState()
        {
            throw new System.NotImplementedException();
        }

        private void PublishGame() 
        {

            GameInfo gi = AskGameInfo();
            // Game info to discovery server
            stateMachine.ChangeState(new GameLobbyState(stateMachine,gi));
       
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