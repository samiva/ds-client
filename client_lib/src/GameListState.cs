using System;
using System.Collections.Generic;

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

		public List<PeerInfo> JoinGame (GameInfo game, P2Pplayer client) {

			bool AbortJoin (GameInfo game) {
				try {
					client.SendQuitGame (game.Ip, game.Port);
				} catch (Exception ex) {
					return false;
				}
				return true;
			}

			bool SendJoin (GameInfo game, in int MAX_RETRIES) {
				bool success = false;
				int i = 0;
				do {
					try {
						client.SendJoinGame (game.Ip, game.Port);
						success = true;
					} catch (Exception ex) {
					}
					++i;
				} while (!success && i < MAX_RETRIES);
				return success;
			}

			bool FetchPeers (GameInfo game, in int MAX_RETRIES, List<PeerInfo> peers) {
				bool success = false;
				int i = 0;
				do {
					try {
						// TODO: Fetch peer list
						success = true;
					} catch (Exception ex) {
					}
					++i;
				} while (!success && i < MAX_RETRIES);
				return success;
			}

			const int MAX_RETRIES = 3;
			List<PeerInfo> peers = new List<PeerInfo>();
			if (
				!SendJoin (game, MAX_RETRIES)
				|| !FetchPeers (game, MAX_RETRIES, peers)
			) {
				AbortJoin (game);
				return null;
			}
			return peers;
		}

	}
}