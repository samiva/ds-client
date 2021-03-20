using System;

using BombPeliLib;

namespace BombPeli
{
	public class PublishGameEventArgs : EventArgs
	{

		public GameInfo game;

		public PublishGameEventArgs (GameInfo game) {
			this.game = game;
		}

	}
}