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

			//ClientPlayer myPlayer = new ClientPlayer(args[0]);

			//RPSGame game = new RPSGame();
			//game.InitGame(myPlayer);
			//game.GameLoop();

			TestMaxClient(100, args[0]);

			Thread.Sleep(1000);
		}


		static void TestMaxClient(int clientNum, string config)
		{
			List<RPSGame> games = new List<RPSGame>();
			List<ClientPlayer> clients = new List<ClientPlayer>();

			ThreadPool.SetMinThreads(clientNum * 3, clientNum * 3);

			for (int i = 0; i < clientNum; ++i)
			{
				var game = new RPSGame();
				var client = new ClientPlayer(config);

				clients.Add(client);
				games.Add(game);
				game.InitGame(client);

				ThreadPool.QueueUserWorkItem(GameStart, game);
			}
			Thread.Sleep(1000);

			for (int i = 0; i < clientNum; ++i)
			{
				games[i].MyStateMachine.HandleMessage("player" + i.ToString());
				games[i].MyStateMachine.HandleMessage("123456");
				Thread.Sleep(100);
			}

			Thread.Sleep(1000);


			for (int round = 0; round < 20; ++round)
			{
				for (int i = 0; i < clientNum; ++i)
				{
					games[i].MyStateMachine.HandleMessage("s");
					Thread.Sleep(100);
				}

				Thread.Sleep(1000);

				for (int i = 0; i < clientNum; ++i)
				{
					clients[i].SetAction();
					games[i].MyStateMachine.HandleMessage(clients[i].GetAction().ToString());
					Thread.Sleep(100);
				}
				Thread.Sleep(3000);
			}
			

			for (int i = 0; i < clientNum; ++i)
			{
				games[i].TestOver();
				Thread.Sleep(50);
			}

			Console.ReadLine();

			games.Clear();
			clients.Clear();
			
			Thread.Sleep(1000);
		}

		static void GameStart(object game)
		{
			(game as RPSGame).GameLoop();
		}
	}
}
