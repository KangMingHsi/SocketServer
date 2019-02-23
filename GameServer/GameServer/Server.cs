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
		public readonly int MaxGameRoom = 50;
		public readonly double UpdateInterval = 60.0;

		private readonly int MaxClient = 100;

		private TcpListener _tcpListener;
		private List<ClientPlayer> _clients;

		private List<GameRoom> _gamingRooms;
		private List<GameRoom> _availableRooms;
		private List<ClientPlayer> _pendingPlayers;

		private bool _isFinish = false;
		private int _port;
		private IPAddress _iPAddress;
		
		private DatabaseHelper _databaseHelper;

		private Timer _timer;

		// TODO create new class to handle byte[] transfer between message

		public Server(string ip, int port)
		{
			_timer = new Timer();

			_clients = new List<ClientPlayer>();

			_port = port;
			_iPAddress = IPAddress.Parse(ip);

			// TODO Need to use param
			string[] postgresConfig = new string[] { "127.0.0.1", "5432", "sean_kang", "jfigames", "train" };
			string[] redisConfig = new string[] { "127.0.0.1", "6379" };

			_databaseHelper = new DatabaseHelper(postgresConfig, redisConfig);

			_tcpListener = new TcpListener(_iPAddress, _port);
			_tcpListener.Start();

			Log.Information("伺服器啟動在Port:{0}", _port.ToString());

			ThreadPool.SetMinThreads(MaxClient * 2 + 4, MaxClient * 2 + 4);
		}

		public void Run()
		{
			try
			{
				SetupGameRooms(MaxGameRoom);
				_timer.Start();

				while (!_isFinish)
				{
					double deltaTime = _timer.DeltaTime;

					ServerCommandHandle();
					ListenToClient();

					CheckGameIsOver();
					MatchPendingPlayers();

					UpdateStatus(deltaTime);

					UpdateData(deltaTime);

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

		public DatabaseHelper GetDatabaseHelper()
		{
			return _databaseHelper;
		}

		private void SetupGameRooms(int roomCnt)
		{
			Log.Information("初始化房間");

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
				_clients.Add(new ClientPlayer(_tcpListener.AcceptTcpClient(), MessageHandle));

				Log.Information("總連線數:{0}", _clients.Count.ToString());
			}	
		}

		// Flexiable To Add New Things
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
					Log.Information("回收房間");
					_availableRooms.Add(_gamingRooms[i]);
					_gamingRooms.RemoveAt(i);
				}
			}
		}

		private void MatchPendingPlayers()
		{
			if (_availableRooms.Count > 0 && _pendingPlayers.Count > 1)
			{
				Log.Information("配對玩家");
				var matching = new PlayerMatching();
				_pendingPlayers.Sort(matching);

				int idxOffset = 0;

				do
				{
					if (Math.Abs(_pendingPlayers[idxOffset].Account.Score - _pendingPlayers[idxOffset + 1].Account.Score) >= 10)
					{
						++idxOffset;
					}
					else
					{
						Log.Information("配對一組成功");
						GameRoom room = _availableRooms[0];

						room.AddPlayer(_pendingPlayers[idxOffset]);
						_pendingPlayers.RemoveAt(idxOffset);
						room.AddPlayer(_pendingPlayers[idxOffset]);
						_pendingPlayers.RemoveAt(idxOffset);
						ThreadPool.QueueUserWorkItem(StartGame, room);

						_gamingRooms.Add(room);
						_availableRooms.RemoveAt(0);
					}
				}
				while (_availableRooms.Count > 0 && (_pendingPlayers.Count - idxOffset) > 1);
			}
		}

		private void UpdateStatus(double deltaTime)
		{
			foreach (var client in _clients)
			{
				break;
			}
		}

		private void UpdateData(double deltaTime)
		{
			_databaseHelper.SynchronizeDatabase(deltaTime);
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
					_databaseHelper.Logout(ref clientPlayer.Account);
					clientPlayer.Disconnect();

					Log.Information("總連線數:{0}", _clients.Count.ToString());
					break;

				case (int)Message.SignIn:

					clientPlayer.Account.Username = messageBuffer.ReadString();
					clientPlayer.Account.Password = messageBuffer.ReadString();

					messageBuffer.Reset();

					_databaseHelper.Login(ref clientPlayer.Account);

					if (clientPlayer.Account.IsOnline)
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
				_databaseHelper.Logout(ref client.Account);
				client.Disconnect();
			}

			_clients.Clear();
			_databaseHelper.Close();

			Log.Information("關機!!");
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


