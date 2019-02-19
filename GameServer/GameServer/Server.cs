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

		private bool _isFinish = false;
		private int _port;
		private IPAddress _iPAddress;

		public Server(string ip, int port)
		{
			_buffer = new byte[BufferSize];
			_data = new StringBuilder();
			_tcpClients = new List<TcpClient>();

			_port = port;
			_iPAddress = IPAddress.Parse(ip);
		}

		~Server()
		{
			Shutdown();
			_tcpListener.Stop();
			_tcpClients.Clear();
		}

		public void Run()
		{
			try
			{
				ThreadPool.SetMinThreads(MaxClient * 2, MaxClient * 2);

				_tcpListener = new TcpListener(_iPAddress, _port);
				_tcpListener.Start();
				Console.WriteLine(@"Use port {0} connect to {1}", _port.ToString(), _iPAddress.ToString());

				ThreadPool.QueueUserWorkItem(ServerCommandHandle, null);

				while (!_isFinish)
				{
					if (_tcpListener.Pending())
					{
						TcpClient client = _tcpListener.AcceptTcpClient();
						_tcpClients.Add(client);
						Console.WriteLine("Total Connected Client: {0}", _tcpClients.Count.ToString());

						ThreadPool.QueueUserWorkItem(ClientReadBegin, client);
					}
				}

				//Shutdown();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.StackTrace.ToString());
			}
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
		}

		private void ServerCommandHandle(object obj)
		{
			while (!_isFinish)
			{
				if (Console.KeyAvailable)
				{
					var key = Console.ReadKey().Key;

					switch (key)
					{
						case ConsoleKey.Q:
							_isFinish = true;
							Broadcast("Bye");
							break;
						case ConsoleKey.H:
							Broadcast("Hi");
							break;
						default:
							break;
					}
				}
			}
		}

		private void Shutdown()
		{
			_tcpListener.Stop();

			foreach (var client in _tcpClients)
			{
				client.Close();
			}

		}

		private void Broadcast(string message)
		{
			foreach (var client in _tcpClients)
			{
				SendMessage(client, message);
			}
		}

		private void SendMessage(TcpClient client, string message)
		{
			ThreadPool.QueueUserWorkItem(ClientWriteBegin, new WrappedClient(client, message));
		}

		private void ClientWriteBegin(object wrappedClient)
		{
			ClientWriteTask task = new ClientWriteTask();
			WrappedClient client = wrappedClient as WrappedClient;

			task.Begin(client.TcpClient, client.Message);
			//Console.WriteLine("Remain Connections: {0}", _curClient.ToString());
		}
	}

	internal class WrappedClient
	{
		public TcpClient TcpClient;
		public string Message;

		public WrappedClient(TcpClient client, string message)
		{
			TcpClient = client;
			Message = message;
		}
	}
}


