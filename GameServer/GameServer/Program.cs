using System.Threading;

using Serilog;

namespace GameServer
{
    class Program
    {
        static void Main(string[] args)
        {
			Log.Logger = new LoggerConfiguration()
						.WriteTo.Console()
						.CreateLogger();

			//string[] config = new string[] {"127.0.0.1", "5432", "sean_kang", "jfigames", "train" };
			//DatabaseConnector db = new DatabaseConnector(config);

			//db.Login("player0", "123456");
			Server server = new Server("127.0.0.1", 36000);
			server.Run();
			Thread.Sleep(2000);
		}

	}
	
}
