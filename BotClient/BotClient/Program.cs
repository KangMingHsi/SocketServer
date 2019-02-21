using System;
using System.Collections.Generic;
using System.Threading;

using GameNetwork;
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


			RPSGame game = new RPSGame();

			game.AddPlayer(new ClientPlayer());
			game.AddPlayer(new ClientPlayer());

			game.GameLoop();

			Thread.Sleep(1000);

			game.AddPlayer(new ClientPlayer());
			game.AddPlayer(new ClientPlayer());

			game.GameLoop();

			//TestMaxClient(100);
			Thread.Sleep(1000);
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
				//Thread.Sleep(1000);
			}

			byte[] b = new byte[100];
			MessageBuffer messageBuffer = new MessageBuffer(b);

			for (int i = 0; i < clientNum; ++i)
			{
				messageBuffer.Position = 0;
				messageBuffer.WriteInt(Message.SignIn);
				messageBuffer.WriteString("player"+i.ToString());
				messageBuffer.WriteString("123456");

				clients[i].Send(messageBuffer.Buffer);
				Thread.Sleep(10);
			}


			Console.ReadLine();

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
