using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace GameServer
{
	class Server
	{
		public delegate void DisconnectHandler(TcpClient client);

		private readonly int BufferSize = 1024;
		private readonly int MaxClient = 100;

		private Byte[] _buffer;
		private StringBuilder _data;

		private TcpListener _tcpListener;
		private List<TcpClient> _tcpClients;

		private Boolean _isFinish = false;

		public Server(string ip, int port)
		{
			_buffer = new byte[BufferSize];
			_data = new StringBuilder();
			_tcpClients = new List<TcpClient>();

			IPAddress iPAddress = IPAddress.Parse(ip);

			try
			{
				ThreadPool.SetMinThreads(MaxClient*2, MaxClient*2); // TODO 參數化數字
				_tcpListener = new TcpListener(iPAddress, port);
				_tcpListener.Start();
				Console.WriteLine(@"Use port {0} connect to {1}", port.ToString(), ip);
				while (!_isFinish)
				{
					TcpClient client = _tcpListener.AcceptTcpClient();
					_tcpClients.Add(client);

					Console.WriteLine("Total Connected Client: {0}", _tcpClients.Count.ToString());
					ThreadPool.QueueUserWorkItem(ClientReadBegin, client);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.StackTrace.ToString());
			}
		}

		~Server()
		{
			_tcpListener.Stop();
			_tcpClients.Clear();
		}

		public void DisconnectEvent(TcpClient client)
		{
			Console.WriteLine("Disconnect");
			_tcpClients.Remove(client);

			Console.WriteLine("Total Connected Client: {0}", _tcpClients.Count.ToString());
		}

		private void ClientReadBegin(object client)
		{
			ClientReadTask task = new ClientReadTask();
			task.Begin(client as TcpClient, DisconnectEvent);
			//Console.WriteLine("Remain Connections: {0}", _curClient.ToString());
		}
	}
}
