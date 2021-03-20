using System;

namespace BombPeliLib
{
    public class P2Pplayer
    {
        private P2PComm p2p;
        public P2Pplayer(P2PComm p2p) {
            this.p2p = p2p;
            this.p2p.DataReceived += P2p_DataReceived;
        }


        public event EventHandler<P2PCommEventArgs> GameStartReceived;
        public event EventHandler<P2PCommEventArgs> GameEndReceived;
        public event EventHandler<P2PCommEventArgs> BombReceived;


        public event EventHandler<P2PCommEventArgs> QuitReceived;
        public event EventHandler<P2PCommEventArgs> JoinReceived;

        private void P2p_DataReceived(object sender, P2PCommEventArgs e)
        {
            if (e.MessageChannel == Channel.GAME)
            {
                if(!e.Data.bomb)
                {
                    if (e.Data.status == GameStatus.RUNNING)
                        OnGameStartReceived(e);
                    else if (e.Data.status == GameStatus.ENDED)
                        OnGameEndReceived(e);
                }else 
                {
                    OnBombReceived(e);
                }
            }
            else if (e.MessageChannel == Channel.MANAGEMENT)
            {
                if(e.Data.msg.equals("join"))
                {
                    OnJoinReceived(e);
                }
                else if (e.Data.msg.equals("quit"))
                {
                    OnQuitReceived(e);
                }
            }
        }

        public void SendBomb(string address, int port)
        {
            p2p.Send(Channel.GAME, new { bomb = true, status=GameStatus.RUNNING }, address, port);
        }

        public void SendStartGame(string address, int port)
        {
            p2p.Send(Channel.GAME, new { bomb = false, status = GameStatus.RUNNING }, address, port);
        }

        public void SendEndGame(string address, int port)
        {
            p2p.Send(Channel.GAME, new { bomb = false, status = GameStatus.ENDED }, address, port);
        }

        public void SendJoinGame(string address, int port)
        {
            p2p.Send(Channel.MANAGEMENT, new { msg = "join" }, address, port);
        }

        public void SendQuitGame(string address, int port)
        {
            p2p.Send(Channel.MANAGEMENT, new { msg = "quit" }, address, port);
        }

        private void OnGameStartReceived(P2PCommEventArgs e)
        {
            GameStartReceived?.Invoke(this, e);
        }

        private void OnBombReceived(P2PCommEventArgs e)
        {
            BombReceived?.Invoke(this,e);
        }

        private void OnJoinReceived(P2PCommEventArgs e)
        {
            JoinReceived?.Invoke(this, e);
        }

        private void OnQuitReceived(P2PCommEventArgs e)
        {
            QuitReceived?.Invoke(this, e);
        }

        private void OnGameEndReceived(P2PCommEventArgs e)
        {
            GameEndReceived?.Invoke(this, e);
        }

    }
}
