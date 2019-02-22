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


			//RPSGame game = new RPSGame();

			//game.AddPlayer(new ClientPlayer());
			//game.AddPlayer(new ClientPlayer());

			//game.GameLoop();

			//Thread.Sleep(1000);

			//game.AddPlayer(new ClientPlayer());
			//game.AddPlayer(new ClientPlayer());

			//game.GameLoop();

			TestMaxClient(2);

			Thread.Sleep(2);
		}

		static void TestMaxClient(int clientNum)
		{
			List<ClientPlayer> clients = new List<ClientPlayer>();

			ThreadPool.SetMinThreads(clientNum * 2 + 4, clientNum * 2 + 4);

			for (int i = 0; i < clientNum; ++i)
			{
				clients.Add(new ClientPlayer());
			}

			for (int i = 0; i < clientNum; ++i)
			{
				clients[i].ConnectToServer();
				Thread.Sleep(10);
			}

			Console.ReadLine();


			MessageBuffer messageBuffer = new MessageBuffer(new byte[100]);

			for (int i = 0; i < clientNum; ++i)
			{
				messageBuffer.Reset();
				messageBuffer.WriteInt((int)Message.SignIn);
				messageBuffer.WriteString("player"+i.ToString());
				messageBuffer.WriteString("123456");

				clients[i].SendMessageToServer(messageBuffer.Buffer);
				Thread.Sleep(10);
			}


			Console.ReadLine();

			for (int i = 0; i < clientNum; ++i)
			{
				messageBuffer.Reset();
				messageBuffer.WriteInt((int)Message.MatchGame);

				clients[i].SendMessageToServer(messageBuffer.Buffer);
				Thread.Sleep(10);
			}


			Thread.Sleep(1000);



			Console.ReadLine();

			for (int i = 0; i < clientNum; ++i)
			{
				clients[i].Disconnect();
				Thread.Sleep(10);
			}

			clients.Clear();
		}
    }
}
