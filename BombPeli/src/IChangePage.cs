using BombPeliLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombPeli
{
	public interface IChangePage
	{

		public void   Init (State? state);
		public void   Clear ();
		public State? GetState ();

	}
}
