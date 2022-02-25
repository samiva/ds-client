using System;

using BombPeliLib;

namespace BombBot
{
	/// <summary>
	/// Interaction logic for ConfigureGame.xaml
	/// </summary>
	public class ConfigureGame
	{

		static public GameInfo? createGame (Config config) {
			ConfigGameState configState = new ConfigGameState (config);
			string gameName = config.GetString ("server_name");
			ushort port     = config.GetUshort ("localport");
			if (gameName.Length < 4) {
				Console.WriteLine ("Name must be at least 4 characters long.");
				return null;
			}
			if (gameName.Length > 30) {
				Console.WriteLine ("Name must be less than 30 characters long.");
				return null;
			}
			GameInfo game = ServiceDiscoveryClient.CreateNewGameInstance (gameName, port);
			try {
				return configState.PublishGame (game);
			} catch (Exception ex) {
				Console.WriteLine (string.Format ("Could not register game. Try publishing again.\n{0}\n\n{1}", ex.Message, ex.StackTrace));
			}
			return null;
		}
		
	}
}
