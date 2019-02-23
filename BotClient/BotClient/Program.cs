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

			ClientPlayer myPlayer = new ClientPlayer(args[0]);

			MessageBuffer messageBuffer = new MessageBuffer(new byte[2048]);

			bool GameOver = false;

			while (!myPlayer.IsOnline && !GameOver)
			{
				Log.Information("請輸入帳號:");
				myPlayer.Account.Username = Console.ReadLine();

				Log.Information("請輸入密碼:");
				myPlayer.Account.Password = Console.ReadLine();

				myPlayer.ConnectToServer();

				while (true) ;
			}

			myPlayer.Disconnect();

			//ThreadPool.QueueUserWorkItem(InputHandle, null);

			//RPSGame game = new RPSGame();
			//game.InitGame(myPlayer);

			Thread.Sleep(1000);
		}


		private static void InputHandle(object obj)
		{
			if (Console.KeyAvailable)
			{
				var key = Console.ReadKey().Key;
				byte[] bytes = null;

				switch (key)
				{
					default:
						break;
				}
			}
		}


		//static void TestMaxClient(int clientNum)
		//{
		//	List<ClientPlayer> clients = new List<ClientPlayer>();
		//	RSAClientProvider rsa = new RSAClientProvider();

		//	ThreadPool.SetMinThreads(clientNum * 2 + 4, clientNum * 2 + 4);

		//	for (int i = 0; i < clientNum; ++i)
		//	{
		//		clients.Add(new ClientPlayer(""));
		//		Thread.Sleep(10);
		//	}
			
		//	for (int i = 0; i < clientNum; ++i)
		//	{
		//		clients[i].ConnectToServer();
		//		Thread.Sleep(10);
		//	}

		//	Console.ReadLine();


		//	MessageBuffer messageBuffer = new MessageBuffer(new byte[2048]);

		//	for (int i = 0; i < clientNum; ++i)
		//	{
		//		messageBuffer.Reset();
		//		messageBuffer.WriteInt((int)Message.SignIn);
		//		messageBuffer.WriteString("player"+i.ToString());
		//		messageBuffer.WriteString(rsa.Encrypt("123456"));

		//		clients[i].SendMessageToServer(messageBuffer.Buffer);
		//		Thread.Sleep(10);
		//	}

		//	Console.ReadLine();

		//	Log.Information("列隊等待中");
		//	for (int i = 0; i < clientNum; ++i)
		//	{
		//		messageBuffer.Reset();
		//		messageBuffer.WriteInt((int)Message.MatchGame);

		//		clients[i].SendMessageToServer(messageBuffer.Buffer);
		//		Thread.Sleep(10);
		//	}


		//	Thread.Sleep(1000);



		//	Console.ReadLine();

		//	for (int i = 0; i < clientNum; ++i)
		//	{
		//		clients[i].Disconnect();
		//		Thread.Sleep(10);
		//	}

		//	clients.Clear();
		//}
    }
}
