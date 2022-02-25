using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace BombPeliLib
{
	public class ServiceDiscoveryClient
	{

		private const string HEADER_ACCEPT       = "Accept";
		private const string HEADER_ACCEPT_VALUE = "application/json";
		private const string REQUEST_MIME_TYPE   = "application/json";
		
		private Config                config;
		private Uri                   serviceLocatorDomain;
		private Uri                   gamesApi;
		private JsonSerializerOptions jsonOpts;

		public ServiceDiscoveryClient (Config config) {
			this.config               = config;
			this.serviceLocatorDomain = new Uri (config.GetString ("server_domain"));
			this.gamesApi             = new Uri (serviceLocatorDomain, "/games");
			this.jsonOpts             = new JsonSerializerOptions (JsonSerializerDefaults.Web);
		}

		static public GameInfo CreateNewGameInstance (string name, ushort port) {
			return new GameInfo (0, name, "", port, GameStatus.OPEN);
		}

		public List<GameInfo> FetchGameList () {
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, gamesApi);
			return ParseGameListResponse (SendRequest (request));
		}

		public GameInfo RegisterGame (GameInfo game) {
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, gamesApi);
			request.Content = new StringContent (SerializeCreateGameInfo (game));
			request.Headers.Add (HEADER_ACCEPT, HEADER_ACCEPT_VALUE);
			request.Content.Headers.ContentType = new MediaTypeHeaderValue (REQUEST_MIME_TYPE);
			return ParseGameCreateResponse (SendRequest (request));
		}

		public void StartGame (GameInfo game) {
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, gamesApi);
			request.Content = new StringContent (SerializeStartGameInfo (game));
			request.Headers.Add (HEADER_ACCEPT, HEADER_ACCEPT_VALUE);
			request.Content.Headers.ContentType = new MediaTypeHeaderValue (REQUEST_MIME_TYPE);
			SendRequest (request);
		}

		public void DeregisterGame (GameInfo game) {
			HttpRequestMessage request = new HttpRequestMessage (HttpMethod.Delete, gamesApi);
			request.Content = new StringContent (SerializeDeregisterGameInfo (game));
			request.Headers.Add (HEADER_ACCEPT, HEADER_ACCEPT_VALUE);
			request.Content.Headers.ContentType = new MediaTypeHeaderValue (REQUEST_MIME_TYPE);
			SendRequest (request);
		}

		private string SendRequest (HttpRequestMessage request) {
			using (HttpClient client = new HttpClient ()) {
				Task<HttpResponseMessage> t = client.SendAsync(request);
				t.Wait();
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
			List<GameInfo>? games = JsonSerializer.Deserialize<List<GameInfo>> (response, this.jsonOpts);
			if (games == null) {
				return new List<GameInfo> ();
			}
			for (
				int i = 0, count = games.Count;
				i < count;
				++i
			) {
				GameInfo game = games [i];
				if (!this.checkGameInfo (game)) {
					games.RemoveAt (i);
					--count;
					--i;
				}
			}
			return games;
		}

		private GameInfo ParseGameCreateResponse (string response) {
			GameInfo game = JsonSerializer.Deserialize<GameInfo> (response, this.jsonOpts);
			if (checkGameInfo (game)) {
				return game;
			}
			throw new Exception ("Invalid create game response.");
		}

		private string SerializeCreateGameInfo (GameInfo info) {
			return JsonSerializer.Serialize (
				new {
					port = info.port,
					name = info.name
				}
			);
		}

		private string SerializeStartGameInfo (GameInfo info) {
			return JsonSerializer.Serialize (
				new {
					id   = info.id,
					port = info.port
				}
			);
		}

		private string SerializeDeregisterGameInfo (GameInfo info) {
			return JsonSerializer.Serialize (
				new {
					id   = info.id,
					port = info.port
				}
			);
		}

		private bool checkGameInfo (GameInfo info) {
			if (info.id <= 0) {
				return false;
			}
			if (info.name.Length == 0) {
				return false;
			}
			if (info.port == 0) {
				return false;
			}
			if (!Enum.IsDefined (typeof (GameStatus), info.status)) {
				return false;
			}
			return true;
		}

	}
}
