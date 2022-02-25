using System;

namespace BombPeliLib.Events
{
	public class BombReceivedEventArgs : EventArgs
	{
		readonly public int bombtime;

		public BombReceivedEventArgs (int bombtime) {
			this.bombtime = bombtime;
		}

	}
}
