using System;
using System.Windows.Controls;

using BombPeliLib;

namespace BombPeli
{
	public class PublishGameEventArgs : EventArgs
	{

		public GameInfo game;
		public Label errorMsg;

		public PublishGameEventArgs (GameInfo game, Label errorMsg) {
			this.game = game;
			this.errorMsg = errorMsg;
		}

	}
}