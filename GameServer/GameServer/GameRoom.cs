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

		private int _roomId;

		public GameRoom(Server server, int id)
		{
			InitWinnerLookUpTable();

			_server = server;
			_messageBuffer = new MessageBuffer(new byte[8]);

			_roomId = id;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}

			return _roomId.Equals((obj as GameRoom).GetId());
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public int GetId()
		{
			return _roomId;
		}

		public void GameStart()
		{
			IsOver = false;

			GameInit();

			while (!IsOver)
			{
				if (_leftPlayer == null || _rightPlayer == null)
				{
					Log.Information("有人斷線");
					break;
				}

				if (_leftPlayer.HasInput() && _rightPlayer.HasInput())
				{
					var leftAction = _leftPlayer.GetAction();
					var rightAction = _rightPlayer.GetAction();

					int result = GameResult(leftAction, rightAction);

					SendGameAction(_leftPlayer, rightAction);
					SendGameAction(_rightPlayer, leftAction);

					if (result == 0)
					{
						Log.Information("平手!");
						IsOver = true; // for test
						//Broadcast(_messageBuffer.Buffer);
					}
					else 
					{
						UpdateScore(result);
						IsOver = true;
					}
					
				}

				Thread.Sleep(1);
			}

			GameOver();
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

		private void GameInit()
		{
			_messageBuffer.Reset();
			_messageBuffer.WriteInt((int)Message.MatchGame);
			Broadcast(_messageBuffer.Buffer);

			_leftPlayer.Account.IsMatch = true;
			_rightPlayer.Account.IsMatch = true;

			Log.Information("遊戲開始");
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

		private void UpdateScore(int result)
		{
			Log.Information("計算成績");

			if (result > 0)
			{
				_leftPlayer.Account.Score += Constant.WinScore;
				_rightPlayer.Account.Score += Constant.LoseSocre;

				Log.Information("玩家{0}獲勝", _leftPlayer.Account.Username);
			}
			else
			{
				_rightPlayer.Account.Score += Constant.WinScore;
				_leftPlayer.Account.Score += Constant.LoseSocre;
				Log.Information("玩家{0}獲勝", _rightPlayer.Account.Username);
			}

			Log.Information("玩家{0}得分:{2}, 玩家{1}得分:{3}", _leftPlayer.Account.Username, _rightPlayer.Account.Username
																,_leftPlayer.Account.Score.ToString(), _rightPlayer.Account.Score.ToString());

			var dbHelper = _server.GetDatabaseHelper();

			dbHelper.UpdateScore(_leftPlayer.Account);
			dbHelper.UpdateScore(_rightPlayer.Account);
		}

		private void SendGameResult(ClientPlayer player)
		{
			_messageBuffer.Reset();
			_messageBuffer.WriteInt((int)Message.GameOver);
			_messageBuffer.WriteInt(player.Account.Score);

			player.SendGameData(_messageBuffer.Buffer.Clone() as byte[]);
		}

		private void SendGameAction(ClientPlayer player, int action)
		{
			_messageBuffer.Reset();
			_messageBuffer.WriteInt((int)Message.GameAction);
			_messageBuffer.WriteInt(action);

			player.SendGameData(_messageBuffer.Buffer.Clone() as byte[]);
		}

		private void GameOver()
		{
			Log.Information("遊戲結束");
			if (_leftPlayer != null)
			{
				SendGameResult(_leftPlayer);
				_leftPlayer = null;
			}

			if (_rightPlayer != null)
			{
				SendGameResult(_rightPlayer);
				_rightPlayer = null;
			}
		}
	}
}
