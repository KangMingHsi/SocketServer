using System;
using System.Collections.Generic;
using System.Threading;

using GameNetwork;
using Serilog;

namespace GameServer
{
	class GameRoom
	{
		public int RoomId;
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

			Broadcast(_messageBuffer.Buffer);

			Log.Information("遊戲開始");

			while (!IsOver)
			{
				if (_leftPlayer.HasInput() && _rightPlayer.HasInput())
				{
					int result = GameResult(_leftPlayer.GetAction(), _rightPlayer.GetAction());

					if (result == 0)
					{
						Log.Information("平手!");
						Broadcast(_messageBuffer.Buffer);
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

		private void Broadcast(byte[] message)
		{
			_leftPlayer.SendGameData(message.Clone() as byte[]);
			_rightPlayer.SendGameData(message.Clone() as byte[]);
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

				Log.Information("初始化規則");
			}
		}

		private int GameResult(int leftPlayerAction, int rightPlayerAction)
		{
			return _winnerLookUpTable[leftPlayerAction, rightPlayerAction];
		}

		private void GameOver(int result)
		{
			Log.Information("遊戲結束");

			_messageBuffer.Reset();
			_messageBuffer.WriteInt((int)Message.GameOver);

			_leftPlayer.SendGameData(_messageBuffer.Buffer);
			_rightPlayer.SendGameData(_messageBuffer.Buffer);

			var dbHelper = _server.GetDatabaseHelper();

			if (result > 0)
			{
				_leftPlayer.Account.Score += _server.WinScore;
				_rightPlayer.Account.Score += _server.LoseSocre;

				Log.Information("玩家{0}獲勝", _leftPlayer.Account.Username);
			}
			else
			{
				_rightPlayer.Account.Score += _server.WinScore;
				_leftPlayer.Account.Score += _server.LoseSocre;
				Log.Information("玩家{0}獲勝", _rightPlayer.Account.Username);
			}

			Log.Information("玩家{0}得分:{2}\n玩家{1}得分:{3}", _leftPlayer.Account.Username, _rightPlayer.Account.Username
																,_leftPlayer.Account.Score.ToString(), _rightPlayer.Account.Score.ToString());

			dbHelper.UpdateScore(_leftPlayer.Account);
			dbHelper.UpdateScore(_rightPlayer.Account);

			_leftPlayer = null;
			_rightPlayer = null;
		}
	}
}
