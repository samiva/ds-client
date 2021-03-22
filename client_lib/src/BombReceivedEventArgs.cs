using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombPeliLib
{
	public class BombReceivedEventArgs : EventArgs
	{
		public int bombtime;

		public BombReceivedEventArgs (int bombtime) {
			this.bombtime = bombtime;
		}

	}
}
