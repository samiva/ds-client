using System;
using System.Collections.Generic;

namespace BombPeliLib
{
    class GameLobbyState :State
    {

        private Config config;
        private GameInfo gameInfo;
        private List<PeerInfo> peerInfos;
        private P2PComm p2p;
        private Role role; 

        public GameLobbyState(StateMachine sm, GameInfo gi, Config config, P2PComm p2p, Role role)
            : base(sm)
        {
            this.gameInfo = gi;
            this.config = config;
            this.p2p = p2p;
            this.role = role;
            
        }

        private void P2p_DataReceived(object sender, P2PCommEventArgs e)
        {
            if (e.MessageChannel == Channel.MANAGEMENT)
            {
                if(role==Role.HOST && peerInfos.Count <= config.GetInt("maxpeer"))
                {
                    PeerInfo peer = new PeerInfo(e.RemoteAddress, e.RemotePort);
                    peerInfos.Add(peer);
                }
            }
            else if(e.MessageChannel==Channel.GAME)
            {
                GameStatus status = e.Data.status;
                if (status == GameStatus.RUNNING)
                {
                    stateMachine.ChangeState(new GameState(stateMachine, p2p));
                }
                else if (status == GameStatus.ENDED)
                {
                    //stateMachine.ChangeState(new GameListState(null, stateMachine, config));
                    LeaveGame();
                }
            }
        }

        public override void BeginState()
        {
            peerInfos = new List<PeerInfo>();
            p2p.DataReceived += P2p_DataReceived;
        }

        public override void ProcessState()
        {
            //DisplayMenu();

            //int choice = int.Parse(Console.ReadLine());

            //switch (choice)
            //{
            //    case 1:
            //        StartGame();
            //        break;
            //    case 2:
            //        LeaveGame();
            //        break;
            //    default:
            //        break;
            //}
        }

        public override void EndState()
        {
            p2p.DataReceived -= P2p_DataReceived;
        }

        private void StartGame()
        {
            // I AM THE ONE HOSTING GAME
            // Inform peers
            foreach(var pi in peerInfos)
            {
                p2p.Send(Channel.GAME, new { status=GameStatus.RUNNING}, pi.Address, pi.Port);
            }
            stateMachine.ChangeState(new GameState(stateMachine, p2p));
        }

        private void LeaveGame()
        {
            Console.WriteLine("Leaving game");

            if (role == Role.HOST)
            {
                foreach(var p in peerInfos)
                {
                    p2p.Send(Channel.GAME, new { status = GameStatus.ENDED }, p.Address, p.Port);
                }
            }

            stateMachine.ChangeState(new GameListState(null, stateMachine, config));
        }

        private void DisplayMenu()
        {
            Console.WriteLine("1) Start game");
            Console.WriteLine("2) Leave game");


        }

    }
}