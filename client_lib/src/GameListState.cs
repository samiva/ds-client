using System;
using System.Collections.Generic;
using kevincastejon;

namespace BombPeliLib
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
            P2PComm p2p = new P2PComm(config.GetUshort("localport"));
            stateMachine.ChangeState(new ConfigGameState(stateMachine, config, p2p));


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