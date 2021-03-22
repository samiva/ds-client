using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombPeliLib
{
	public class StartGameEventArgs : EventArgs
	{
		public readonly GameLobbyState lobby;

		public StartGameEventArgs (GameLobbyState lobby) {
			this.lobby = lobby;
		}

	}
}
