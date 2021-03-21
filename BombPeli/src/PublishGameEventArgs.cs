using System;
using System.Windows.Controls;

using BombPeliLib;

namespace BombPeli
{
	public class PublishGameEventArgs : EventArgs
	{

		public ConfigGameState configState;
		public GameInfo game;
		public Label errorMsg;

		public PublishGameEventArgs (ConfigGameState configState, GameInfo game, Label errorMsg) {
			this.configState = configState;
			this.game = game;
			this.errorMsg = errorMsg;
		}

	}
}