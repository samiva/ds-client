using System;

namespace BombPeliLib.Events
{
	sealed public class GameStartEventArgs : EventArgs
	{

		readonly public int bombtime;

		public GameStartEventArgs (int bombtime) {
			this.bombtime = bombtime;
		}
	}
}
