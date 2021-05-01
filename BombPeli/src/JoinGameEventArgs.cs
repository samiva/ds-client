using System;
using System.Windows.Controls;

using BombPeliLib;

namespace BombPeli
{
	public class JoinGameEventArgs : EventArgs
	{

		public GameInfo game;
		public Label errorMsg;

		public JoinGameEventArgs (GameInfo game, Label errorMsg) {
			this.game = game;
			this.errorMsg = errorMsg;
		}

	}
}
