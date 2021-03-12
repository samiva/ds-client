using System.Collections.Generic;

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
            throw new System.NotImplementedException();
        }

        public override void ProcessState()
        {
            throw new System.NotImplementedException();
        }

        public override void EndState()
        {
            throw new System.NotImplementedException();
        }

        void CreateGame()
        {

        }

        void JoinGame()
        {

        }

        void RefreshList()
        {

        }

        void DisplayGameList()
        {

        }

    }
}