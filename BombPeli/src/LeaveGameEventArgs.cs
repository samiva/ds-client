using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BombPeliLib;

namespace BombPeli
{
	public class LeaveGameEventArgs : EventArgs
	{
		public readonly GameLobbyState lobby;

		public LeaveGameEventArgs (GameLobbyState lobby) {
			this.lobby = lobby;
		}

	}
}
