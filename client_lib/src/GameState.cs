namespace BombPeliLib
{
    class GameState : State
    {
        private P2PComm p2p;
        private bool bomb = false;
        public GameState(StateMachine sm, P2PComm p2p) :base(sm)
        {
            this.p2p = p2p;
            this.p2p.DataReceived += P2p_DataReceived;
        }

        private void P2p_DataReceived(object sender, P2PCommEventArgs e)
        {
            if(e.MessageChannel == Channel.GAME)
            {
               bomb = e.Data.bomb;
            }
        }

        public override void BeginState()
        {
            throw new System.NotImplementedException();
        }

        public override void ProcessState()
        {
            if (bomb)
            {
                PassBomb();
            }
        }

        public override void EndState()
        {
            p2p.DataReceived -= P2p_DataReceived;
        }

        void PassBomb()
        {
            //p2p.Send(Channel.GAME, new { bomb = this.bomb }, )
        }

        void LeaveGame() 
        {

        }
    }
}