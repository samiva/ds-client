using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BombPeliLib;

namespace BombPeli
{
	public class LeaveGameEventArgs
	{
		public GameState game;

		public LeaveGameEventArgs (GameState game) {
			this.game = game;
		}

	}
}
