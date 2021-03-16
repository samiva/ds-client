﻿using BombPeli;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BombPeli
{
	class ServiceDiscoveryClient
	{
		private Config config;
		private Uri serviceLocatorDomain;
		private Uri gamesApi;

		public ServiceDiscoveryClient (Config config) {
			this.config = config;
			this.serviceLocatorDomain = new Uri (config.GetString ("server_domain"));
			this.gamesApi = new Uri (serviceLocatorDomain, "/games");
		}

		public List<GameInfo> FetchGameList () {
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, gamesApi);
			return ParseGameListResponse (SendRequest (request));
		}

		public GameInfo RegisterGame (GameInfo game) {
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, gamesApi);
			request.Content = new StringContent (SerializeCreateGameInfo (game));
			return ParseGameCreateResponse (SendRequest (request));
		}

		public void StartGame (GameInfo game) {
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, gamesApi);
			request.Content = new StringContent (SerializeStartGameInfo (game));
			SendRequest (request);
		}

		public void DeregisterGame (GameInfo game) {
			HttpRequestMessage request = new HttpRequestMessage (HttpMethod.Delete, gamesApi);
			request.Content = new StringContent (SerializeDeregisterGameInfo (game));
			SendRequest (request);
		}

		private string SendRequest (HttpRequestMessage request) {
			using (HttpClient client = new HttpClient ()) {
				Task<HttpResponseMessage> t = client.SendAsync(request);
				bool done = t.Wait (5000);
				if (!done) {
					throw new Exception ("Failed to send request.");
				}
				HttpResponseMessage response = t.Result;
				int code = (int)response.StatusCode;
				if (!(100 <= code && code < 400)) {
					throw new Exception ("Failure response code.");
				}
				Task<string> data = response.Content.ReadAsStringAsync ();
				data.Wait ();
				return data.Result;
			}
		}

		private List<GameInfo> ParseGameListResponse (string response) {
			List<GameInfo> games = JsonConvert.DeserializeObject<List<GameInfo>> (response);
			for (
				int i = 0, count = games.Count;
				i < count;
				++i
			) {
				GameInfo game = games [i];
				if (!checkGameInfo (game)) {
					games.RemoveAt (i);
					--count;
					--i;
				}
			}
			return games;
		}

		private GameInfo ParseGameCreateResponse (string response) {
			GameInfo game = JsonConvert.DeserializeObject<GameInfo>(response);
			if (checkGameInfo (game)) {
				return game;
			}
			throw new Exception ("Invalid create game response.");
		}

		private string SerializeCreateGameInfo (GameInfo info) {
			return JsonConvert.SerializeObject (new {
				port = info.Port,
				name = info.Name
			}, Formatting.None);
		}

		private string SerializeStartGameInfo (GameInfo info) {
			return JsonConvert.SerializeObject (new {
				id = info.Id
			}, Formatting.None);
		}

		private string SerializeDeregisterGameInfo (GameInfo info) {
			return JsonConvert.SerializeObject (new {
				id = info.Id
			}, Formatting.None);
		}

		private bool checkGameInfo (GameInfo info) {
			if (info.Id <= 0) {
				return false;
			}
			if (info.Name == null || info.Name.Length == 0) {
				return false;
			}
			if (info.Port == 0) {
				return false;
			}
			if (!Enum.IsDefined (typeof (GameStatus), info.Status)) {
				return false;
			}
			return true;
		}

	}
}
