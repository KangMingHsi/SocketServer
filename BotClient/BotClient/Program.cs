using System;
using System.Collections.Generic;
using System.Threading;

namespace BotClient
{
    class Program
    {
        static void Main(string[] args)
        {

			TestMaxClient(100);

		}

		static void TestMaxClient(int clientNum)
		{
			List<Client> clients = new List<Client>();

			for (int i = 0; i < clientNum; ++i)
			{
				clients.Add(new Client("127.0.0.1", 36000));
			}

			for (int i = 0; i < clientNum; ++i)
			{
				clients[i].Connect();
			}

			Thread.Sleep(5000);

			for (int i = 0; i < clientNum; ++i)
			{
				clients[i].Close();
				//Thread.Sleep(100);
			}

			clients.Clear();
		}
    }
}
