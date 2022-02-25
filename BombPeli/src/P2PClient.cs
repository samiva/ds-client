﻿using BombPeliLib;

using System;

namespace BombPeli
{
	/// <summary>
	/// Singleton wrapper for P2PPlayer class to make it easier
	/// to manage the lifetime of the P2PPlayer instance and
	/// release the resources it uses.
	/// </summary>
	public class P2PClient
	{

		static private P2PClient? instance;
		public         P2PApi     client;

		private P2PClient (Config config, bool isHost) {
			ushort port = config.GetUshort ("localport");
			this.client = new P2PApi (port, isHost);
			this.client.Open ();
		}

		~P2PClient () {
			Destroy ();
		}

		static public void Create (Config config, bool isHost) {
			Release ();
			instance = new P2PClient (config, isHost);
		}

		static public P2PClient Instance () {
			return instance ?? throw new Exception ("No P2P client instance.");
		}

		static public void Release () {
			if (instance == null) {
				return;
			}
			instance.Destroy ();
			instance = null;
		}

		private void Destroy () {
			client.Close ();
		}
	}
}
