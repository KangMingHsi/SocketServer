using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;

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

			TestMaxClient(2);
			Thread.Sleep(1000);
		}

		static void TestMaxClient(int clientNum)
		{
			List<ClientPlayer> clients = new List<ClientPlayer>();
			string key;

			using (FileStream file = new FileStream("PublicKey.xml", FileMode.Open, FileAccess.Read))
			{
				byte[] bytes = new byte[1024];
				file.Read(bytes, 0, 1024);
				key = System.Text.Encoding.ASCII.GetString(bytes);
			}

			ThreadPool.SetMinThreads(clientNum * 2 + 4, clientNum * 2 + 4);

			for (int i = 0; i < clientNum; ++i)
			{
				clients.Add(new ClientPlayer());
				Thread.Sleep(10);
			}

			for (int i = 0; i < clientNum; ++i)
			{
				clients[i].ConnectToServer();
				Thread.Sleep(10);
			}

			Console.ReadLine();


			MessageBuffer messageBuffer = new MessageBuffer(new byte[2048]);

			for (int i = 0; i < clientNum; ++i)
			{
				messageBuffer.Reset();
				messageBuffer.WriteInt((int)Message.SignIn);
				messageBuffer.WriteString("player"+i.ToString());
				messageBuffer.WriteString(RSAHelper.Encrypt(key, "123456"));

				clients[i].SendMessageToServer(messageBuffer.Buffer);
				Thread.Sleep(10);
			}

			Console.ReadLine();

			Log.Information("列隊等待中");
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
