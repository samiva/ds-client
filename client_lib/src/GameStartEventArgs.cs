using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombPeliLib
{
	public class GameStartEventArgs
	{

		public readonly int bombTime;

		public GameStartEventArgs (int bombTime) {
			this.bombTime = bombTime;
		}
	}
}
