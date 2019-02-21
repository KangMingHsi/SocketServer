using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using GameCommon;

namespace GameServer
{
	class Server
	{
		private readonly int MaxClient = 100;

		private TcpListener _tcpListener;
		private List<CustomClient> _clients;

		private bool _isFinish = false;
		private int _port;
		private IPAddress _iPAddress;

		public Server(string ip, int port)
		{
			_clients = new List<CustomClient>();

			_port = port;
			_iPAddress = IPAddress.Parse(ip);
		}

		~Server()
		{
			Shutdown();
		}

		public void Run()
		{
			try
			{
				_tcpListener = new TcpListener(_iPAddress, _port);
				_tcpListener.Start();
				Console.WriteLine(@"Server is on and binds to port {0}", _port.ToString());

				ThreadPool.SetMinThreads(MaxClient * 2, MaxClient * 2);
				ThreadPool.QueueUserWorkItem(ServerCommandHandle, null);

				ListenToClient();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.StackTrace.ToString());
			}
		}

		public void MessageHandle(CustomClient client, byte[] message)
		{
			MessageBuffer messageBuffer = new MessageBuffer(message);
			int messageType = messageBuffer.ReadInt();

			switch (messageType)
			{
				case Message.Disconnect:
					// TODO save data to database
					_clients.Remove(client);
					client.Stop();
					break;
				case Message.SignIn:

					// TODO Refactorize and link to database
					string usrname = messageBuffer.ReadString();
					string password = messageBuffer.ReadString();

					byte[] response = new byte[10];
					messageBuffer.Reset(response);

					if (usrname.Equals("seankang") && password.Equals("123456"))
					{
						messageBuffer.WriteInt(Message.SignInSuccess);
					}
					else
					{
						messageBuffer.WriteInt(Message.SignInFail);
					}

					SendMessage(client, messageBuffer.Buffer);
					break;
				default:
					break;
			}

		}

		private void ListenToClient()
		{
			try
			{
				while (!_isFinish)
				{
					if (_tcpListener.Pending())
					{
						TcpClient tcpClient = _tcpListener.AcceptTcpClient();
						CustomClient client = new CustomClient(tcpClient, MessageHandle);
						_clients.Add(client);
						Console.WriteLine("Total Connected Client: {0}", _clients.Count.ToString());
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.StackTrace.ToString());
				throw;
			}
		}

		// TODO 
		private void ServerCommandHandle(object obj)
		{
			while (!_isFinish)
			{
				if (Console.KeyAvailable)
				{
					var key = Console.ReadKey().Key;

					byte[] message = new byte[10];
					MessageBuffer messageBuffer = new MessageBuffer(message);

					switch (key)
					{
						case ConsoleKey.Q:
							messageBuffer.WriteInt(Message.Disconnect);
							Broadcast(messageBuffer.Buffer);

							_isFinish = true;
							break;
						case ConsoleKey.H:

							messageBuffer.WriteInt(Message.NoMeaning);
							Broadcast(messageBuffer.Buffer);
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

			foreach (var client in _clients)
			{
				client.Stop();
			}

			_clients.Clear();
		}

		private void Broadcast(byte[] message)
		{
			foreach (var client in _clients)
			{
				SendMessage(client, message);
			}
		}

		private void SendMessage(CustomClient client, byte[] message)
		{
			client.Send(message);
		}
	}
}


