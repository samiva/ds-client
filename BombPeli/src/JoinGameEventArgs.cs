using System;

using BombPeliLib;

namespace BombPeli
{
	public class JoinGameEventArgs : EventArgs
	{

		public GameInfo game;

		public JoinGameEventArgs (GameInfo game) {
			this.game = game;
		}

	}
}
