using System;
using System.Collections.Generic;
using System.Text;

using Serilog;

namespace BotClient
{
	class RPSGame
	{
		private static int[,] _winnerLookUpTable = null;

		private ClientPlayer _myPlayer = null;
		private ClientPlayer _opponentPlayer = null;

		public RPSGame()
		{
			InitWinnerLookUpTable();
		}

		public void InitGame(ClientPlayer myPlayer)
		{
			_myPlayer = myPlayer;
		}

		public void GameLoop()
		{
			bool isOver = false;

			while (!isOver)
			{
				isOver = true;

				int result = GameResult(_myPlayer.GetAction(), _opponentPlayer.GetAction());

				if (result == 1)
				{
					Log.Information("Pl win");
				}
				else if (result == 0)
				{
					Log.Information("Again!");
					isOver = false;
				}
				else
				{
					Log.Information("P2 win");
				}
			}

			GameOver();
		}

		public bool AddPlayer(ClientPlayer player)
		{
			if (_opponentPlayer == null)
			{
				_opponentPlayer = player;
				return true;
			}

			return false;
		}

		public bool IsReady()
		{
			return  (_opponentPlayer != null);
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

		public int GameResult(int leftPlayerAction, int rightPlayerAction)
		{
			return _winnerLookUpTable[leftPlayerAction, rightPlayerAction];
		}

		private void GameOver()
		{
			_opponentPlayer = null;
		}
	}
}
