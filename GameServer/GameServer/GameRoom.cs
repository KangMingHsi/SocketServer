using System;
using System.Collections.Generic;
using System.Threading;

using GameNetwork;
using Serilog;

namespace GameServer
{
	class GameRoom
	{
		public bool IsOver { get; private set; } = true;

		private static int[,] _winnerLookUpTable = null;
		private Server _server;

		private ClientPlayer _leftPlayer = null;
		private ClientPlayer _rightPlayer = null;

		private MessageBuffer _messageBuffer;

		public GameRoom(Server server)
		{
			InitWinnerLookUpTable();

			_server = server;
			_messageBuffer = new MessageBuffer(new byte[8]);
		}

		public void GameStart()
		{
			IsOver = false;

			_messageBuffer.Reset();
			_messageBuffer.WriteInt((int)Message.MatchGame);

			_leftPlayer.SendGameData(_messageBuffer.Buffer.Clone() as byte[]);
			_rightPlayer.SendGameData(_messageBuffer.Buffer.Clone() as byte[]);

			Log.Information("Game Start");

			while (!IsOver)
			{
				if (_leftPlayer.HasInput() && _rightPlayer.HasInput())
				{
					int result = GameResult(_leftPlayer.GetAction(), _rightPlayer.GetAction());

					if (result == 0)
					{
						Log.Information("Again!");
						_leftPlayer.SendGameData(_messageBuffer.Buffer.Clone() as byte[]);
						_rightPlayer.SendGameData(_messageBuffer.Buffer.Clone() as byte[]);
					}
					else 
					{
						GameOver(result);
						IsOver = true;
					}
					
				}

				Thread.Sleep(1);
			}

		}

		public bool AddPlayer(ClientPlayer player)
		{
			if (_leftPlayer != null && _rightPlayer != null)
			{
				return false;
			}
			else if (_leftPlayer == null)
			{
				_leftPlayer = player;
			}
			else
			{
				_rightPlayer = player;
			}

			return true;
		}

		public bool IsReady()
		{
			return (_leftPlayer != null) && (_rightPlayer != null);
		}

		// 0 = 石頭, 1 = 剪刀, 2 = 布
		private void InitWinnerLookUpTable()
		{
			if (_winnerLookUpTable == null)
			{
				_winnerLookUpTable = new int[3, 3];

				for (int i = 0; i < 3; ++i)
				{
					_winnerLookUpTable[i, i] = 0;
				}

				_winnerLookUpTable[0, 1] = 1;
				_winnerLookUpTable[0, 2] = -1;

				_winnerLookUpTable[1, 0] = -1;
				_winnerLookUpTable[1, 2] = 1;

				_winnerLookUpTable[2, 0] = 1;
				_winnerLookUpTable[2, 1] = -1;

				Log.Information("Init");
			}
		}

		private int GameResult(int leftPlayerAction, int rightPlayerAction)
		{
			return _winnerLookUpTable[leftPlayerAction, rightPlayerAction];
		}

		private void GameOver(int result)
		{
			Log.Information("GameOver");

			_messageBuffer.Reset();
			_messageBuffer.WriteInt((int)Message.GameOver);

			_leftPlayer.SendGameData(_messageBuffer.Buffer);
			_rightPlayer.SendGameData(_messageBuffer.Buffer);

			var dbConnector = _server.GetDatabaseConnectior();

			if (result > 0)
			{
				_leftPlayer.Account.Score += _server.WinScore;
				_rightPlayer.Account.Score += _server.LoseSocre;
			}
			else
			{
				_rightPlayer.Account.Score += _server.WinScore;
				_leftPlayer.Account.Score += _server.LoseSocre;
			}

			dbConnector.UpdateScore(_leftPlayer.Account);
			dbConnector.UpdateScore(_rightPlayer.Account);

			_leftPlayer = null;
			_rightPlayer = null;
		}
	}
}
