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

		private TcpListener _tcpListener;
		private List<ClientPlayer> _clients;
		private List<ClientPlayer> _pendingPlayers;

		private RoomManager _roomManager;
		private DatabaseHelper _databaseHelper;
		private Timer _timer;

		private bool _isFinish = false;
		private int _port;
		private IPAddress _iPAddress;
		

		public Server(string serverConfigPath, string postgresConfigPath, string redisConfigPath)
		{
			string[] serverConfig = Config.ReadServerConfig(serverConfigPath);
			string[] postgresConfig = Config.ReadPostgresConfig(postgresConfigPath);
			string[] redisConfig = Config.ReadRedisConfig(redisConfigPath);

			_iPAddress = IPAddress.Parse(serverConfig[0]);
			_port = Int32.Parse(serverConfig[1]);

			_tcpListener = new TcpListener(_iPAddress, _port);
			_tcpListener.Start();

			_databaseHelper = new DatabaseHelper(postgresConfig, redisConfig);
			_roomManager = new RoomManager();

			Log.Information("伺服器啟動在Port:{0}", _port.ToString());

			ThreadPool.SetMinThreads(Constant.MaxClient * 3, Constant.MaxClient * 3);
		}

		public void Run()
		{
			try
			{
				SetupGame();

				while (!_isFinish)
				{
					CommandHandle();
					ListenToClient();

					_timer.Update();
					_roomManager.Update(_pendingPlayers);

					UpdateStatus(_timer.DeltaTime);
					UpdateData(_timer.DeltaTime);

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

		private void SetupGame()
		{
			Log.Information("初始化遊戲服務");

			_timer = new Timer();
			_timer.Start();

			_clients = new List<ClientPlayer>();
			_pendingPlayers = new List<ClientPlayer>();

			_roomManager.Init(this);
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
		private void CommandHandle()
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

		private void UpdateStatus(double deltaTime)
		{
			for (int idx = 0; idx < _clients.Count; ++idx)
			{
				_clients[idx].Update(deltaTime);
			}
		}

		private void UpdateData(double deltaTime)
		{
			_databaseHelper.SynchronizeDatabase(deltaTime);
		}

		private void MessageHandle(ClientPlayer clientPlayer, byte[] message)
		{
			MessageBuffer messageBuffer = new MessageBuffer(message);
			int messageType = messageBuffer.ReadInt();

			switch (messageType)
			{
				case (int)Message.Disconnect:

					if (clientPlayer.Account.IsOnline)
					{
						_databaseHelper.Logout(ref clientPlayer.Account);

						HandleIfInGameOrPending(clientPlayer);
					}

					clientPlayer.Disconnect();
					_clients.Remove(clientPlayer);

					Log.Information("總連線數:{0}", _clients.Count.ToString());
					break;

				case (int)Message.SignIn:

					clientPlayer.Account.Username = messageBuffer.ReadString();
					clientPlayer.Account.Password = messageBuffer.ReadString();

					_databaseHelper.Login(ref clientPlayer.Account);
					messageBuffer.Reset();

					if (clientPlayer.Account.IsOnline)
					{
						messageBuffer.WriteInt((int)Message.SignInSuccess);
						messageBuffer.WriteInt(clientPlayer.Account.Score);
					}
					else
					{
						messageBuffer.WriteInt((int)Message.SignInFail);
					}
					
					SendMessage(clientPlayer, messageBuffer.Buffer);
					break;

				case (int)Message.MatchGame:
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

			for (int i = 0; i < _clients.Count; ++i)
			{
				_databaseHelper.Logout(ref _clients[i].Account);
				_clients[i].Disconnect();
			}

			_clients.Clear();
			_pendingPlayers.Clear();

			_roomManager.Clear();
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

		private void HandleIfInGameOrPending(ClientPlayer player)
		{
			if (_pendingPlayers.Contains(player))
			{
				_pendingPlayers.Remove(player);
			}

			if (player.Account.IsMatch)
			{

			}
		}
	}
}


