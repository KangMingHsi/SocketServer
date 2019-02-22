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

			Server server = new Server("127.0.0.1", 36000);
			server.Run();


			Thread.Sleep(2000);
		}

	}
	
}
