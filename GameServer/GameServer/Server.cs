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
		public delegate void ServerMessageHandler(ClientPlayer clientPlayer, byte[] message);

		public readonly int WinScore = 5;
		public readonly int LoseSocre = -3;

		private readonly int MaxClient = 100;

		private TcpListener _tcpListener;
		private List<ClientPlayer> _clients;

		private List<GameRoom> _gamingRooms;
		private List<GameRoom> _availableRooms;
		private List<ClientPlayer> _pendingPlayers;


		private bool _isFinish = false;
		private int _port;
		private IPAddress _iPAddress;

		private DatabaseConnector _dbConnector;

		// TODO create new class to handle byte[] transfer between message

		public Server(string ip, int port)
		{
			_clients = new List<ClientPlayer>();

			_port = port;
			_iPAddress = IPAddress.Parse(ip);

			// TODO Need to use param
			string[] config = new string[] { "127.0.0.1", "5432", "sean_kang", "jfigames", "train" };
			_dbConnector = new DatabaseConnector(config);

			_tcpListener = new TcpListener(_iPAddress, _port);
			_tcpListener.Start();
			Log.Information(@"Server is on and binds to port {0}", _port.ToString());

			ThreadPool.SetMinThreads(MaxClient * 2 + 4, MaxClient * 2 + 4);
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

		public DatabaseConnector GetDatabaseConnectior()
		{
			return _dbConnector;
		}

		private void SetupGameRooms(int roomCnt)
		{
			_pendingPlayers = new List<ClientPlayer>();
			_gamingRooms = new List<GameRoom>();
			_availableRooms = new List<GameRoom>();

			for (int room = 0; room < roomCnt; ++room)
			{
				_availableRooms.Add(new GameRoom(this)); 
			}
		}

		private void ListenToClient()
		{
			if (_tcpListener.Pending())
			{
				TcpClient tcpClient = _tcpListener.AcceptTcpClient();
		
				_clients.Add(new ClientPlayer(tcpClient, MessageHandle));
				Log.Information("Total Connected Client: {0}", _clients.Count.ToString());
			}	
		}

		// TODO 
		private void ServerCommandHandle()
		{
			if (Console.KeyAvailable)
			{
				var key = Console.ReadKey().Key;
				byte[] bytes = null;
				
				switch (key)
				{
					case ConsoleKey.Q:
						bytes = BitConverter.GetBytes((int)Message.Disconnect);
						Broadcast(bytes);

						_isFinish = true;
						break;
					case ConsoleKey.H:
						bytes = BitConverter.GetBytes((int)Message.NoMeaning);
						Broadcast(bytes);
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
				room.AddPlayer(_pendingPlayers[0]);
				_pendingPlayers.RemoveAt(0);
				room.AddPlayer(_pendingPlayers[0]);
				_pendingPlayers.RemoveAt(0);
				ThreadPool.QueueUserWorkItem(StartGame, room);

				_gamingRooms.Add(room);
				_availableRooms.RemoveAt(0);

			}
		}

		private void StartGame(object game)
		{
			GameRoom gRoom = game as GameRoom;
			gRoom.GameStart();
		}

		private void MessageHandle(ClientPlayer clientPlayer, byte[] message)
		{
			MessageBuffer messageBuffer = new MessageBuffer(message);
			int messageType = messageBuffer.ReadInt();

			switch (messageType)
			{
				case (int)Message.Disconnect:
					// TODO save data to database
					_clients.Remove(clientPlayer);
					_dbConnector.Logout(clientPlayer.Account);
					clientPlayer.Disconnect();

					Log.Information("Total Connected Client: {0}", _clients.Count.ToString());
					break;

				case (int)Message.SignIn:

					clientPlayer.Account.Username = messageBuffer.ReadString();
					clientPlayer.Account.Password = messageBuffer.ReadString();

					messageBuffer.Reset();
				
					if (_dbConnector.Login(clientPlayer.Account))
					{
						messageBuffer.WriteInt((int)Message.SignInSuccess);
					}
					else
					{
						messageBuffer.WriteInt((int)Message.SignInFail);
					}

					SendMessage(clientPlayer, messageBuffer.Buffer);
					break;

				case (int)Message.MatchGame:

					// TODO
					// Add to pend list

					Log.Information("Some Start to pend");
					_pendingPlayers.Add(clientPlayer);

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
				_dbConnector.Logout(client.Account);
				client.Disconnect();
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

		private void SendMessage(ClientPlayer client, byte[] message)
		{
			client.SendGameData(message);
		}
	}
}


