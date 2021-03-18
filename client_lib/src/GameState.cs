namespace BombPeli
{
    class GameState : State
    {
        private P2PComm p2p;
        public GameState(StateMachine sm, P2PComm p2p) :base(sm)
        {
            this.p2p = p2p;
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

        void PassBomb()
        {

        }

        void LeaveGame() 
        {

        }
    }
}