using System;
using System.Collections.Generic;
using kevincastejon;

namespace BombPeli
{
    class GameLobbyState :State
    {

        private Config config;
        private GameInfo gameInfo;
        private List<PeerInfo> peerInfos;
        private P2PComm p2p;

        public GameLobbyState(StateMachine sm, GameInfo gi, Config config, P2PComm p2p)
            : base(sm)
        {
            this.gameInfo = gi;
            this.config = config;
            this.p2p = p2p;
        }

        public override void BeginState()
        {
            peerInfos = new List<PeerInfo>();

        }

        public override void ProcessState()
        {
            DisplayMenu();

            int choice = int.Parse(Console.ReadLine());

            switch (choice)
            {
                case 1:
                    StartGame();
                    break;
                case 2:
                    LeaveGame();
                    break;
                default:
                    break;
            }
        }

        public override void EndState()
        {
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
            stateMachine.ChangeState(new GameListState(null, stateMachine, config));
        }

        private void DisplayMenu()
        {
            Console.WriteLine("1) Start game");
            Console.WriteLine("2) Leave game");


        }

    }
}