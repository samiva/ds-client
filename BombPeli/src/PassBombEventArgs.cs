using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BombPeliLib;

namespace BombPeli
{
	public class PassBombEventArgs
	{
		public GameState game;

		public PassBombEventArgs (GameState game) {
			this.game = game;
		}

	}
}
