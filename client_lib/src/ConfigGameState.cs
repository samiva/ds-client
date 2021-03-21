using System;

namespace BombPeliLib
{
	public class ConfigGameState
	{

		private Config config;

		public ConfigGameState (Config config) {
			this.config = config;
		}

		public GameInfo PublishGame (GameInfo game) {
			ServiceDiscoveryClient service = new ServiceDiscoveryClient(config);
			return service.RegisterGame (game);
		}

	}
}