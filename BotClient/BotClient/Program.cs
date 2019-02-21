using System;
using System.Collections.Generic;
using System.Threading;

using Serilog;

namespace BotClient
{
    class Program
    {
        static void Main(string[] args)
        {
			Log.Logger = new LoggerConfiguration()
						.WriteTo.Console()
						.CreateLogger();

			TestMaxClient(100);

		}

		static void TestMaxClient(int clientNum)
		{
			List<CustomClient> clients = new List<CustomClient>();

			ThreadPool.SetMinThreads(clientNum * 2, clientNum * 2);

			for (int i = 0; i < clientNum; ++i)
			{
				clients.Add(new CustomClient("127.0.0.1", 36000));
			}

			for (int i = 0; i < clientNum; ++i)
			{
				clients[i].Connect();
			}

			Thread.Sleep(1000);
			for (int i = 0; i < clientNum; ++i)
			{
				clients[i].Close();
				Thread.Sleep(1);
				clients[i].Stop();
			}

			clients.Clear();
		}
    }
}
