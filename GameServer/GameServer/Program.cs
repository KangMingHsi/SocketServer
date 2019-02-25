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


			Server server = new Server(args[0], args[1], args[2]);
			server.Run();

			Thread.Sleep(2000);
		}
	}
}
