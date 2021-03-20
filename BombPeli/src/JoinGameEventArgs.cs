using System;
using System.Windows.Controls;

using BombPeliLib;

namespace BombPeli
{
	public class JoinGameEventArgs : EventArgs
	{

		public GameListState gameList;
		public GameInfo game;
		public Label errorMsg;

		public JoinGameEventArgs (GameListState gameList, GameInfo game, Label errorMsg) {
			this.gameList = gameList;
			this.game = game;
			this.errorMsg = errorMsg;
		}

	}
}
