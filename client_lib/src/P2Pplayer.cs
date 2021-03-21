using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Newtonsoft.Json;

namespace BombPeliLib
{
    public class P2Pplayer
    {
        private readonly P2PComm p2p;
        private List<PeerInfo> peers;

        public P2Pplayer(P2PComm p2p)
        {
            this.p2p = p2p;
            this.p2p.DataReceived += P2p_DataReceived;
            peers = new List<PeerInfo>();
        }

        public event EventHandler<P2PCommEventArgs> GameStartReceived;
        public event EventHandler<P2PCommEventArgs> GameEndReceived;
        public event EventHandler<P2PCommEventArgs> BombReceived;
        public event EventHandler<P2PCommEventArgs> QuitReceived;
        public event EventHandler<P2PCommEventArgs> JoinReceived;

        private void P2p_DataReceived(object sender, P2PCommEventArgs e)
        {
            Console.WriteLine(e.MessageChannel);
            switch (e.MessageChannel)
            {
                case Channel.GAME: ProcessGameDataMsg(e); break;
                case Channel.MANAGEMENT: ProcessManagementDataMsg(e); break;
            }
        }

        private void ProcessGameDataMsg(P2PCommEventArgs e)
        {

            //Console.WriteLine("DATA OBJECT");
            GameStatus status = (GameStatus)e.Data.status;
            bool bomb = (bool)e.Data.bomb;


            if (bomb)
            {
                OnBombReceived(e);
            }
            else
            {
                switch (status)
                {
                    case GameStatus.RUNNING: OnGameStartReceived(e); break;
                    case GameStatus.ENDED: OnGameEndReceived(e); break;
                }
            }

        }

        private void ProcessManagementDataMsg(P2PCommEventArgs e)
        {
            Console.WriteLine("MANAGEMENT MSG");
            dynamic tmp = e.Data.msg;
            //Console.WriteLine(tmp.ToString());
            //Console.WriteLine(tmp.Equals("quit", StringComparison.OrdinalIgnoreCase));
            string t = (string)tmp; // tmp.ToString(); // Can as string be used?

            if (t is string msg)
            {
                if (msg.Equals("join", StringComparison.OrdinalIgnoreCase))
                {
                    OnJoinReceived(e);
                }
                else if (msg.Equals("quit", StringComparison.OrdinalIgnoreCase))
                {
                    //Console.WriteLine("QUIT MSG");
                    OnQuitReceived(e);
                }
                else if (msg.Equals("list_peers", StringComparison.OrdinalIgnoreCase))
                {
                    //Console.WriteLine("PEER REQUEST");
                    OnListPeers(e);
                }
                else if (msg.Equals("peers", StringComparison.OrdinalIgnoreCase))
                {
                    //Console.WriteLine("PEER LIST RECEIVED");
                    string peermsg = e.Data.peerlist.ToString();
                    OnPeerListReceived(peermsg);
                    Console.WriteLine(peermsg);
                }
            }
        }

        public void SendBomb(string address, int port)
        {
            p2p.Send(Channel.GAME, new
            {
                bomb = true,
                status = GameStatus.RUNNING
            }, address, port);
        }

        public void SendStartGame(string address, int port)
        {
            p2p.Send(Channel.GAME, new
            {
                bomb = false,
                status = GameStatus.RUNNING
            }, address, port);
        }

        // Send when game is won and ends
        public void SendEndGame(string address, int port)
        {
            p2p.Send(Channel.GAME, new
            {
                bomb = false,
                status = GameStatus.ENDED
            }, address, port);
        }

        public void SendJoinGame(string address, int port)
        {
            p2p.Send(Channel.MANAGEMENT, new
            {
                msg = "join"
            }, address, port);
        }

        // Send when client leaves network
        public void SendQuitGame(string address, int port)
        {
            p2p.Send(Channel.MANAGEMENT, new
            {
                msg = "quit"
            }, address, port);
        }

        public void SendPeerRequest(string address, int port)
        {
            p2p.Send(Channel.MANAGEMENT, new { msg = "list_peers" }, address, port);
        }
        public void Close()
        {
            p2p.Close();
        }

        private void OnGameStartReceived(P2PCommEventArgs e)
        {
            GameStartReceived?.Invoke(this, e);
        }

        private void OnBombReceived(P2PCommEventArgs e)
        {
            BombReceived?.Invoke(this, e);
        }

        private void OnJoinReceived(P2PCommEventArgs e)
        {
            AddPeer(e.RemoteAddress, e.RemotePort);
            JoinReceived?.Invoke(this, e);
        }

        private void OnQuitReceived(P2PCommEventArgs e)
        {
            RemovePeer(e.RemoteAddress, e.RemotePort);
            QuitReceived?.Invoke(this, e);
        }

        private void OnGameEndReceived(P2PCommEventArgs e)
        {
            GameEndReceived?.Invoke(this, e);
        }

        private void AddPeer(string address, int port)
        {
            Console.WriteLine(String.Format("{0}:{1}", address,port));
            PeerInfo pi = new PeerInfo(address, port);
            if (!peers.Contains(pi, new PeerInfoComparer()))
            {
                peers.Add(pi);
            }
        }

        private void RemovePeer(string address, int port)
        {
            peers.Remove(new PeerInfo(address, port));
        }

        private void OnListPeers(P2PCommEventArgs e)
        {
            //peers.Add(new PeerInfo("127.0.0.1", 666));
            //peers.Add(new PeerInfo("156.272.488.60", 8888));

            // TODO: Add self to peer list before sending
            AddPeer("xxxxxxxxxxxxx", 666);
            string json = JsonConvert.SerializeObject(peers, Formatting.Indented);
            p2p.Send(Channel.MANAGEMENT, new { msg = "peers", peerlist = json }, e.RemoteAddress, e.RemotePort);
            Console.WriteLine("PEERS SENT");
        }

        private void OnPeerListReceived(string peermsg)
        {
            peers = JsonConvert.DeserializeObject<List<PeerInfo>>(peermsg);
            //Console.WriteLine(p.Count);
            //foreach(var i in p)
            //         {
            //	Console.WriteLine(String.Format("{0}:{1}", i.Address, i.Port));
            //         }
        }

    }
}
