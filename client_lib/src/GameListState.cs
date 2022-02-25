using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
			get { return games; }
			set { games = value; }
		}

		public void JoinGame (GameInfo game, P2PApi client) {
			const int  MAX_ATTEMPTS = 5;
			const int  delay        = 200;
			int        attempts     = 0;
			Task<bool> result;
			do {
				result = client.DoJoinGame (game.getEndpoint ());
				result.Wait ();
				if (result.Result) {
					break;
				}
				++attempts;
				if (attempts >= MAX_ATTEMPTS) {
					throw new Exception ("Failed to join game.");
				}
				Thread.Sleep (delay);
			} while (true);
			attempts = 0;
			do {
				result = client.DoRequestPeerList (game.getEndpoint ());
				result.Wait ();
				if (result.Result) {
					break;
				}
				++attempts;
				if (attempts >= MAX_ATTEMPTS) {
					throw new Exception ("Failed to request peer list.");
				}
				Thread.Sleep (delay);
			} while (true);
		}

	}
}