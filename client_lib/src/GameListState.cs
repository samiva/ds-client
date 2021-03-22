using System;
using System.Collections.Generic;
using System.Threading;

using kevincastejon;

namespace BombPeliLib
{
	public class GameListState : State
	{
		private List<GameInfo> games;
		private Config config;

		public GameListState (List<GameInfo> games, Config config) {
			this.config = config;
			this.games = games;
		}

		public List<GameInfo> Games {
			get {
				return games;
			}
			set {
				games = value;
			}
		}

		public void JoinGame (GameInfo game, P2Pplayer client) {
			client.SendListPeers (game.Ip, game.Port);
			client.SendJoinGame (game.Ip, game.Port);
		}

	}
}