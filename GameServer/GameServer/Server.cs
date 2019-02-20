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
		public delegate void MessageHandler(ClientTask client, byte[] message);

		private readonly int BufferSize = 1024;
		private readonly int MaxClient = 100;

		private TcpListener _tcpListener;
		private List<TcpClient> _tcpClients;

		private bool _isFinish = false;
		private int _port;
		private IPAddress _iPAddress;

		public Server(string ip, int port)
		{
			_tcpClients = new List<TcpClient>();

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
				Console.WriteLine(@"Server is on and binds to {0}", _port.ToString());

				ThreadPool.SetMinThreads(MaxClient * 2, MaxClient * 2);
				ThreadPool.QueueUserWorkItem(ServerCommandHandle, null);

				ListenToClient();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.StackTrace.ToString());
			}
		}

		public void MessageHandle(ClientTask client, byte[] message)
		{
			MessageBuffer messageBuffer = new MessageBuffer(message);
			int messageType = messageBuffer.ReadInt();
			TcpClient tcpClient = client.GetTcpClient();

			switch (messageType)
			{
				case Message.Disconnect:
					// TODO save data to database
					_tcpClients.Remove(tcpClient);
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

					SendMessage(tcpClient, messageBuffer.Buffer);
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
						TcpClient client = _tcpListener.AcceptTcpClient();
						_tcpClients.Add(client);
						Console.WriteLine("Total Connected Client: {0}", _tcpClients.Count.ToString());

						ThreadPool.QueueUserWorkItem(ClientReadBegin, client);
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.StackTrace.ToString());
				throw;
			}
		}

		private void ClientReadBegin(object client)
		{
			ClientReadTask task = new ClientReadTask(client as TcpClient, MessageHandle);
			task.Begin();
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

			foreach (var client in _tcpClients)
			{
				client.Close();
			}

			_tcpClients.Clear();
		}

		private void Broadcast(byte[] message)
		{
			foreach (var client in _tcpClients)
			{
				SendMessage(client, message);
			}
		}

		private void SendMessage(TcpClient client, byte[] message)
		{
			ThreadPool.QueueUserWorkItem(ClientWriteBegin, new WrappedClient(client, message));
		}

		private void ClientWriteBegin(object wrappedClient)
		{
			WrappedClient client = wrappedClient as WrappedClient;
			ClientWriteTask task = new ClientWriteTask(client.TcpClient, client.Message, MessageHandle);
	
			task.Begin();
		}
	}

	internal class WrappedClient
	{
		public TcpClient TcpClient;
		public byte[] Message;

		public WrappedClient(TcpClient client, byte[] message)
		{
			TcpClient = client;
			Message = message;
		}
	}
}


