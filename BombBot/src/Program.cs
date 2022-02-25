using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using BombPeliLib;

namespace BombBot
{
	class Program
	{
		static void makeConfigs () {
			string template = @"[client]
; Fully qualified domain name
server_domain=http://127.0.0.1:5000
; Local UDP P2P network port
localport={0}
; Bomb lifetime in milliseconds
bomb_lifetime=5000
; Server game name
server_name=game-{1}
; Print network message log
print_msg_log = false
";
			int    portNumber = 12345;
			for (int j = 0; j < 8; ++j) {
				for (int i = 0; i < 1001; ++i) {
					string config = string.Format (template, portNumber, j);
					File.WriteAllText ($"configs/config_{j}_{i}.ini", config);
					++portNumber;
				}
			}
		}

		static void processOutput () {
			string   outputDir = "output";
			foreach (string dir in Directory.GetDirectories (outputDir)) {
				StringBuilder result = new StringBuilder ();
				result.Append ("game,sent,received,duration,\n");
				foreach (string filePath in Directory.GetFiles (dir)) {
					string game = Path.GetFileName (filePath).Split ('_')[1];
					result.Append (game).Append (',');
					foreach (string line in File.ReadAllLines (filePath)) {
						result.Append (line.Split (':') [1].Trim ()).Append (',');
					}
					result.Append ('\n');
				}
				File.WriteAllText (Path.Join (outputDir,Path.GetFileName (dir) + "result"), result.ToString());
			}
			
		}

		static int Main (string[] args) {
			Bootstrap bs       = new Bootstrap ();
			if (!bs.init (args, out string configFile)) {
				Console.WriteLine (bs.getError);
				return 1;
			}
			bool       isHost = bs.getIsHost;
			MainWindow window = new MainWindow (bs.getConfig);
			if (isHost) {
				hostRoutine (bs, window, configFile);
			} else {
				playerRoutine (bs, window, configFile);
			}
			return 0;
		}

		static private bool hostRoutine (Bootstrap bs, MainWindow window, string configFile) {
			Config    config = bs.getConfig;
			GameInfo? gi   = ConfigureGame.createGame (config);
			if (gi == null) {
				return false;
			}
			P2PApi? client = window.makeClient (config, true);
			if (client == null) {
				return false;
			}
			GameLobby lobby = new GameLobby (gi.Value, bs.getConfig, client, bs.getIsHost, bs.getPlayerCapacity);
			lobby.init (client);
			bool success = lobby.waitForPlayers ();
			lobby.release (client);
			if (!success) {
				return false;
			}
			Stopwatch      timer      = new Stopwatch ();
			GameLobbyState lobbyState = lobby.getState;
			Game           game       = new Game (config, client, lobbyState.Game, lobbyState.BombTime);
			game.init (client);
			timer.Start ();
			game.start ();
			game.waitForGameEnd ();
			timer.Stop ();
			game.release (client);
			window.releaseClient (client);
			string stats = string.Format (
				"Packets sent: {0}\nPackets received: {1}\nGame duration: {2} ms",
				client.getPacketSendCount, client.getPacketReceiveCount,
				timer.ElapsedMilliseconds
			);
			Console.WriteLine(stats);
			File.WriteAllText ("output/" + Path.GetFileName (configFile) + "output", stats);
			client.Close ();
			return true;
		}

		static private bool playerRoutine (Bootstrap bs, MainWindow window, string configFile) {
			Config    config = bs.getConfig;
			GameList  list   = new GameList (config);
			GameInfo? gi     = list.findGameServer ();
			if (gi == null) {
				return false;
			}
			P2PApi? client = window.makeClient (config, false);
			if (client == null) {
				return false;
			}
			GameLobby lobby = new GameLobby (gi.Value, bs.getConfig, client, bs.getIsHost, bs.getPlayerCapacity);
			lobby.init (client);
			if (!list.joinGame (gi.Value, client)) {
				return false;
			}
			bool success = lobby.waitForPlayers ();
			lobby.release (client);
			if (!success) {
				return false;
			}
			Stopwatch      timer      = new Stopwatch ();
			GameLobbyState lobbyState = lobby.getState;
			Game           game       = new Game (config, client, lobbyState.Game, lobbyState.BombTime);
			game.init (client);
			timer.Start ();
			game.waitForGameEnd ();
			timer.Stop ();
			game.release (client);
			window.releaseClient (client);
			string stats = string.Format (
				"Packets sent: {0}\nPackets received: {1}\nGame duration: {2} ms",
				client.getPacketSendCount, client.getPacketReceiveCount,
				timer.ElapsedMilliseconds
			);
			Console.WriteLine(stats);
			File.WriteAllText ("output/" + Path.GetFileName (configFile) + "output", stats);
			client.Close ();
			return true;
		}

	}
}
