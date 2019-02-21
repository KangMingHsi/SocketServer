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
		private List<ClientNetwork> _clients;

		private List<GameRoom> _gamingRooms;
		private List<GameRoom> _availableRooms;

		private List<int> _pendingPlayers;


		private bool _isFinish = false;
		private int _port;
		private IPAddress _iPAddress;

		private DatabaseConnector _dbConnector;

		public Server(string ip, int port)
		{
			_clients = new List<ClientNetwork>();

			_port = port;
			_iPAddress = IPAddress.Parse(ip);

			// TODO Need to use param
			string[] config = new string[] { "127.0.0.1", "5432", "sean_kang", "jfigames", "train" };
			_dbConnector = new DatabaseConnector(config);

			_tcpListener = new TcpListener(_iPAddress, _port);
			_tcpListener.Start();
			Log.Information(@"Server is on and binds to port {0}", _port.ToString());

			ThreadPool.SetMinThreads(MaxClient * 2 + 1, MaxClient * 2 + 1);
		}

		~Server()
		{
			Shutdown();
		}

		public void Run()
		{
			try
			{
				SetupGameRooms(2);

				while (!_isFinish)
				{
					ServerCommandHandle();
					ListenToClient();

					CheckGameIsOver();
					MatchPendingPlayers();
					


					Thread.Sleep(1);
				}

			}
			catch (Exception e)
			{
				Log.Error(e.StackTrace.ToString());
			}
			finally
			{
				Shutdown();
			}
		}

		private void SetupGameRooms(int roomCnt)
		{
			_gamingRooms = new List<GameRoom>();
			_availableRooms = new List<GameRoom>();

			for (int room = 0; room < roomCnt; ++room)
			{
				_availableRooms.Add(new GameRoom()); 
			}
		}

		private void ListenToClient()
		{
			if (_tcpListener.Pending())
			{
				TcpClient tcpClient = _tcpListener.AcceptTcpClient();
				ClientNetwork client = new ClientNetwork(tcpClient, MessageHandle);
				_clients.Add(client);
				Log.Information("Total Connected Client: {0}", _clients.Count.ToString());
			}	
		}

		// TODO 
		private void ServerCommandHandle()
		{
			if (Console.KeyAvailable)
			{
				var key = Console.ReadKey().Key;

				byte[] message = new byte[10];
				MessageBuffer messageBuffer = new MessageBuffer(message);

				switch (key)
				{
					case ConsoleKey.Q:
						messageBuffer.WriteInt((int)Message.Disconnect);
						Broadcast(messageBuffer.Buffer);

						_isFinish = true;
						break;
					case ConsoleKey.H:

						messageBuffer.WriteInt((int)Message.NoMeaning);
						Broadcast(messageBuffer.Buffer);
						break;
					default:
						break;
				}
			}
		}

		private void CheckGameIsOver()
		{
			for (int i = _gamingRooms.Count-1; i >= 0; --i)
			{
				if(_gamingRooms[i].IsOver) 
				{
					_availableRooms.Add(_gamingRooms[i]);
					_gamingRooms.RemoveAt(i);
				}
			}
		}

		private void MatchPendingPlayers()
		{
			// TODO match opponent by score and must match in 5 second if there is other pending player (no matter the diff of their score) 
			// _pendingPlayers.Sort();

			while (_availableRooms.Count > 0 && _pendingPlayers.Count > 1)
			{
				GameRoom room = _availableRooms[0];

				// TODO
				// Use threads to run game and fix errors
				//room.AddPlayer(_pendingPlayers[0]);
				//_pendingPlayers.RemoveAt(0);
				//room.AddPlayer(_pendingPlayers[0]);
				//_pendingPlayers.RemoveAt(0);
				//room.GameStart();

				_gamingRooms.Add(room);
				_availableRooms.RemoveAt(0);
			}
		}

		private void MessageHandle(ClientNetwork client, byte[] message)
		{
			MessageBuffer messageBuffer = new MessageBuffer(message);
			int messageType = messageBuffer.ReadInt();

			switch (messageType)
			{
				case (int)Message.Disconnect:
					// TODO save data to database
					_clients.Remove(client);
					_dbConnector.Logout(client.Account.Username, client.Account.Password);
					client.Stop();

					Log.Information("Total Connected Client: {0}", _clients.Count.ToString());
					break;
				case (int)Message.SignIn:

					// TODO Refactorize and link to database
					string username = messageBuffer.ReadString();
					string password = messageBuffer.ReadString();

					byte[] response = new byte[10];
					messageBuffer.Reset(response);

					Log.Information("usr:{0}, pwd:{1}", username, password);

					if (_dbConnector.Login(username, password))
					{
						messageBuffer.WriteInt((int)Message.SignInSuccess);
						client.Account.Username = username;
						client.Account.Password = password;
					}
					else
					{
						messageBuffer.WriteInt((int)Message.SignInFail);
					}

					SendMessage(client, messageBuffer.Buffer);
					break;
				case (int)Message.MatchGame:

					// TODO
					// Add to pend list

					break;
				default:
					Log.Information(messageType.ToString());
					break;
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
			Thread.Sleep(100);
		}

		private void Broadcast(byte[] message)
		{
			foreach (var client in _clients)
			{
				SendMessage(client, message);
			}
		}

		private void SendMessage(ClientNetwork client, byte[] message)
		{
			client.Send(message);
		}
	}
}


