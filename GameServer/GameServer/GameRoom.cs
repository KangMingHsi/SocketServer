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

		private ClientPlayer _leftPlayer = null;
		private ClientPlayer _rightPlayer = null;

		public GameRoom()
		{
			InitWinnerLookUpTable();
		}

		public void GameStart()
		{
			IsOver = false;

			byte[] buffer = new byte[100];
			MessageBuffer messageBuffer = new MessageBuffer(buffer);

			while (!IsOver)
			{
				if (_leftPlayer.HasInput() && _rightPlayer.HasInput())
				{
					int result = GameResult(_leftPlayer.GetAction(), _rightPlayer.GetAction());

					if (result == 0)
					{
						Log.Information("Again!");
						continue;
					}
					else if (result == -1)
					{
						Log.Information("P2 win");
						GameOver();
						IsOver = true;
					}
					else
					{
						Log.Information("Pl win");
						GameOver();
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

		private void GameOver()
		{
			_leftPlayer = null;
			_rightPlayer = null;
		}
	}
}
