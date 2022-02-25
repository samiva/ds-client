using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

using BombPeliLib;

namespace BombBot
{
    /// <summary>
    /// Interaction logic for GameList.xaml
    /// </summary>
    public class GameList
    {

        private GameListState gameList;
        private Config        config;

        public GameList (Config config) {
            this.gameList = new GameListState (new List<GameInfo> (), config);;
            this.config   = config;
        }

        public GameListState GetState {
            get { return this.gameList; }
        }

        public GameInfo? findGameServer () {
            const int MAX_RETRIES = 10;
            int       attempts    = 0;
            string    gameName    = config.GetString ("server_name");
            GameInfo? server      = null;
            do {
                List<GameInfo>? games = this.fetchGames ();
                if (games != null) {
                    server = this.findGame (games, gameName);
                }
                if (server == null) {
                    Thread.Sleep (200);
                } else {
                    break;
                }
                ++attempts;
            } while (attempts < MAX_RETRIES);
            return server;
        }
        
		private List<GameInfo>? fetchGames () {
            ServiceDiscoveryClient client = new ServiceDiscoveryClient (this.config);
            List<GameInfo> games;
            try {
                games = client.FetchGameList ();
            } catch (Exception ex) {
                Console.WriteLine ("Failed to refresh game list.\n{0}\n\n{1}", ex.Message, ex.StackTrace);
                return null;
            }
            return games;
        }

        private GameInfo? findGame (List<GameInfo> games, string gameName) {
            foreach (GameInfo game in games) {
                if (game.name == gameName) {
                    return game;
                }
            }
            return null;
        }

        public bool joinGame (GameInfo game, P2PApi client) {
            try {
                this.gameList.JoinGame (game, client);
            } catch (Exception ex) {
                Console.WriteLine ("Could not join game.\n{0}\n\n{1}", ex.Message, ex.StackTrace);
                return false;
            }
            return true;
        }
        

	}
}
