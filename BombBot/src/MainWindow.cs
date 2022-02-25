using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

using BombPeliLib;

using SimpleP2P;

namespace BombBot
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public class MainWindow
	{

		readonly private Config config;
		private          bool   printMsgLog;

		public MainWindow (Config config) {
			this.config = config;
			this.config.TryGetBool ("print_msg_log", out this.printMsgLog);
		}

		public P2PApi? makeClient (Config config, bool isHost) {
			try {
				P2PClient.Create (config, isHost);
			} catch (Exception e) {
				Console.WriteLine ("Could not initiate P2P node.\n{0}\n\n{1}", e.Message, e.StackTrace);
				return null;
			}
			P2PClient.Create (config, isHost);
			P2PApi client = P2PClient.Instance ().client;
			client.PacketReceived += this.MessageLogReceiveHandle;
			client.PacketSent     += this.MessageLogSendHandle;
			return client;
		}

		public void releaseClient (P2PApi client) {
			client.PacketReceived -= this.MessageLogReceiveHandle;
			client.PacketSent     -= this.MessageLogSendHandle;
		}

		public void MessageLogSendHandle(object? sender, ConnectionEventArgs e) {
			this.MessageLogHandle (sender, e, true);
		}

		public void MessageLogReceiveHandle(object? sender, ConnectionEventArgs e) {
			this.MessageLogHandle (sender, e, false);
		}

		private void MessageLogHandle(object? sender, ConnectionEventArgs e, bool send) {
			string        direction = send ? "sent" : "received";
			StringBuilder str       = new StringBuilder ();
			str.Append ('[').Append (DateTime.Now.ToString ()).Append (" ").Append (direction).Append ("] ")
			   .Append (e.peer).Append (" ").Append (Encoding.ASCII.GetString (e.data)).Append ('\n');
			this.logNetworkMsg (str.ToString ());
		}
		
		private void logNetworkMsg (string msg) {
			if (this.printMsgLog) {
				Console.Write (msg);
			}
		}
	}
}
