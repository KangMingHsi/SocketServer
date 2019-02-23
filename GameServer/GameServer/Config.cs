using System.IO;

using Newtonsoft.Json;

namespace GameServer
{
	class Config
	{
		struct ServerConfig
		{
			public string Ip;
			public string Port;
		};

		struct PostgresConfig
		{
			public string Ip;
			public string Port;
			public string User;
			public string Password;
			public string Database;
		};

		struct RedisConfig
		{
			public string Ip;
			public string Port;
		};

		public static string[] ReadServerConfig(string path)
		{
			using (StreamReader r = new StreamReader(path))
			{
				var json = r.ReadToEnd();
				var item = JsonConvert.DeserializeObject<ServerConfig>(json);
				
				return new string[] { item.Ip, item.Port };
			}
		}

		public static string[] ReadRedisConfig(string path)
		{
			using (StreamReader r = new StreamReader(path))
			{
				var json = r.ReadToEnd();
				var item = JsonConvert.DeserializeObject<RedisConfig>(json);

				return new string[] { item.Ip, item.Port };
			}
		}

		public static string[] ReadPostgresConfig(string path)
		{
			using (StreamReader r = new StreamReader(path))
			{
				var json = r.ReadToEnd();
				var item = JsonConvert.DeserializeObject<PostgresConfig>(json);

				return new string[] { item.Ip, item.Port , item.User, item.Password, item.Database};
			}
		}
	}


}
