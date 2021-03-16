using System;
using System.Collections.Generic;
using kevincastejon;

namespace BombPeli
{
    class GameListState : State
    {
        private List<GameInfo> games;
        private Config config;

        public GameListState(List<GameInfo> games, StateMachine sm, Config config) : base(sm)
        {
            this.config = config;
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

            stateMachine.ChangeState(new ConfigGameState(stateMachine, config));


        }

        void JoinGame()
        {
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