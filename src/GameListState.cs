using System;
using System.Collections.Generic;
using kevincastejon;

namespace BombPeli
{
    class GameListState : State
    {
        private List<GameInfo> games;
        public GameListState(List<GameInfo> games, StateMachine sm) : base(sm)
        {
            
            this.games = games;
        }

        public override void BeginState()
        {
            if (games == null)
            {
                RefreshList();
            }
        }

        public override void ProcessState()
        {
            throw new System.NotImplementedException();
        }

        public override void EndState()
        {
            throw new System.NotImplementedException();
        }

        public void CreateGame()
        {

            Config.MyRole = Role.HOST;
            stateMachine.ChangeState(new ConfigGameState(stateMachine));


        }

        void JoinGame()
        {
            Config.MyRole = Role.NORMAL;
        }

        void RefreshList()
        {
            Console.WriteLine("Refresh game list");
        }

        void DisplayGameList()  
        {

        }

        

    }
}