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
        private UDPManager udpm;

        public GameLobbyState(StateMachine sm, GameInfo gi, Config config)
            : base(sm)
        {
            this.gameInfo = gi;
            this.config = config;
        }

        public override void BeginState()
        {
            udpm = new UDPManager(config.GetUshort ("localport"));
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
                udpm.Send("game", new { status=GameStatus.RUNNING}, pi.Address, pi.Port);
            }
            stateMachine.ChangeState(new GameState(stateMachine));
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