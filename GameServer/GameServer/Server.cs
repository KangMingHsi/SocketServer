using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Serilog;
using GameNetwork;

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

		private DatabaseConnector _dbConnector;

		public Server(string ip, int port)
		{
			_clients = new List<CustomClient>();

			_port = port;
			_iPAddress = IPAddress.Parse(ip);

			string[] config = new string[] { "127.0.0.1", "5432", "sean_kang", "jfigames", "train" };
			_dbConnector = new DatabaseConnector(config);
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
				Log.Information(@"Server is on and binds to port {0}", _port.ToString());

				ThreadPool.SetMinThreads(MaxClient * 2, MaxClient * 2);
				ThreadPool.QueueUserWorkItem(ServerCommandHandle, null);

				ListenToClient();
			}
			catch (Exception e)
			{
				Log.Error(e.StackTrace.ToString());
				Shutdown();
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
					_dbConnector.Logout(client.ClientAccount.Username, client.ClientAccount.Password);
					client.Stop();

					Log.Information("Total Connected Client: {0}", _clients.Count.ToString());
					break;
				case Message.SignIn:

					// TODO Refactorize and link to database
					string username = messageBuffer.ReadString();
					string password = messageBuffer.ReadString();

					byte[] response = new byte[10];
					messageBuffer.Reset(response);

					Log.Information("usr:{0}, pwd:{1}", username, password);

					if (_dbConnector.Login(username, password))
					{
						messageBuffer.WriteInt(Message.SignInSuccess);
						client.ClientAccount.Username = username;
						client.ClientAccount.Password = password;
					}
					else
					{
						messageBuffer.WriteInt(Message.SignInFail);
					}

					SendMessage(client, messageBuffer.Buffer);
					break;
				default:
					Log.Information(messageType.ToString());
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
						Log.Information("Total Connected Client: {0}", _clients.Count.ToString());
					}

					Thread.Sleep(1);
				}

				Shutdown();
			}
			catch (Exception e)
			{
				Log.Error(e.StackTrace.ToString());
				throw;
			}
		}

		// TODO 
		private void ServerCommandHandle(object obj)
		{
			try
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

					Thread.Sleep(1);
				}
			}
			catch (Exception e)
			{
				Shutdown();
				Log.Error(e.Message);
			}
		}

		private void Shutdown()
		{
			_tcpListener.Stop();

			foreach (var client in _clients)
			{
				_dbConnector.Logout(client.ClientAccount.Username, client.ClientAccount.Password);
				client.Stop();
			}

			_clients.Clear();
			_dbConnector.Close();

			Log.Information("Finish!!");
			Thread.Sleep(1000);
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


