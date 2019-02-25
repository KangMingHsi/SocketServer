using System.IO;

using Newtonsoft.Json;

namespace BotClient
{
	class Config
	{
		struct ClientConfig
		{
			public string Ip;
			public string Port;
		};

		public static string[] ReadPlayerConfig(string path)
		{
			using (StreamReader r = new StreamReader(path))
			{
				var json = r.ReadToEnd();
				var item = JsonConvert.DeserializeObject<ClientConfig>(json);
				
				return new string[] { item.Ip, item.Port };
			}
		}
	}


}
