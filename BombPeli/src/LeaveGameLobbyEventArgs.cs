using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BombPeliLib;

namespace BombPeli
{
	public class LeaveGameLobbyEventArgs : EventArgs
	{
		public readonly GameLobbyState lobby;

		public LeaveGameLobbyEventArgs (GameLobbyState lobby) {
			this.lobby = lobby;
		}

	}
}
