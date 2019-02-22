using System.Threading;
using System.IO;

using Serilog;
using Npgsql;

namespace GameServer
{
    class Program
    {
        static void Main(string[] args)
        {
			Log.Logger = new LoggerConfiguration()
						.WriteTo.Console()
						.CreateLogger();

			Server server = new Server("127.0.0.1", 36000);
			server.Run();

			//string pwd = "123456";
			//byte[] bytes = new byte[1024];

			//string xmlPublicKey, xmlKeys;

			//using (FileStream file = new FileStream("PublicKey.xml", FileMode.Open, FileAccess.Read))
			//{
			//	file.Read(bytes, 0, 1024);
			//	xmlPublicKey = System.Text.Encoding.ASCII.GetString(bytes);
			//}

			//using (FileStream file = new FileStream("PrivateKey.xml", FileMode.Open, FileAccess.Read))
			//{
			//	file.Read(bytes, 0, 1024);
			//	xmlKeys = System.Text.Encoding.ASCII.GetString(bytes);
			//}

			//var encrypt = RSAHelper.Encrypt(xmlPublicKey, pwd);
			//var decrypt = RSAHelper.Decrypt(xmlKeys, encrypt);

			//string[] config = new string[] { "127.0.0.1", "5432", "sean_kang", "jfigames", "train" };
			////DatabaseConnector dbConnector = new DatabaseConnector(config);
			//Log.Information(encrypt);
			//using (var conn = new NpgsqlConnection(DatabaseCmd.GetConnectionConfig(config)))
			//{
			//	conn.Open();
			//	using (var cmd = new NpgsqlCommand())
			//	{
			//		string password;

			//		cmd.Connection = conn;
			//		cmd.CommandText = string.Format("SELECT password FROM public.player WHERE username='player0';");

			//		using (var reader = cmd.ExecuteReader())
			//		{
			//			reader.Read();
			//			password = reader.GetString(0);
			//		}

			//		Log.Information(RSAHelper.Decrypt(xmlKeys, password).Equals(decrypt).ToString());
			//	}
			//	conn.Close();
			//}

			Thread.Sleep(2000);
		}
	}
	
}
