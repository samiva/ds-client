using System;

namespace BombPeliLib
{
	public class ConfigGameState
	{

		private Config config;

		public ConfigGameState (Config config) {
			this.config = config;
		}

		public void PublishGame (GameInfo game) {
			ServiceDiscoveryClient service = new ServiceDiscoveryClient(config);
			service.RegisterGame (game);
		}

	}
}