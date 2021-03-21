namespace BombPeliLib
{
	public class GameState : State
	{
		private P2Pplayer client;
		private bool hasBomb = false;
		private long bombTime;

		public GameState (P2Pplayer p2p) {
			this.client = p2p;
		}

		~GameState () {
			Destroy ();
		}

		public void ReceiveBomb () {
			hasBomb = true;
		}

		public void PassBomb () {
			if (!hasBomb) {
				return;
			}
			// TODO: Get random peer
			// client.SendBomb ();
			hasBomb = false;
		}

		public void LeaveGame () {
			Destroy ();
		}

		public void FailPassBomb () {
			hasBomb = true;
		}

		private void Destroy () {
			if (client == null) {
				return;
			}
			client = null;
		}
	}
}