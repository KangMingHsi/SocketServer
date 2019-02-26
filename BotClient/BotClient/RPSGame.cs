using System;
using System.Collections.Generic;
using System.Threading;

using Serilog;

namespace BotClient
{
	class RPSGame
	{
		public ClientPlayer MyPlayer { get; private set; }

		private static int[,] _winnerLookUpTable = null;
		private bool _IsOver;
		private int _opponentAction;

		public RPSGame()
		{
			InitWinnerLookUpTable();
			_IsOver = false;
		}

		public void InitGame(ClientPlayer myPlayer)
		{
			myPlayer.Game = this;
			MyPlayer = myPlayer;
		}

		public void GameLoop()
		{
			while (!_IsOver)
			{
				//InputHandle();
				MyPlayer.Update();
				Thread.Sleep(1);
			}

			GameOver();
		}

		public void SetOpponentAction(int action)
		{
			_opponentAction = action;
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
			}
		}

		public int GameResult()
		{
			return _winnerLookUpTable[MyPlayer.GetAction(), _opponentAction];
		}

		private void InputHandle()
		{
			if (Console.KeyAvailable)
			{
				var line = Console.ReadLine();

				if (line.Equals("Q", StringComparison.CurrentCultureIgnoreCase))
				{
					TestOver();
				}
				else
				{
					MyPlayer.MyStateMachine.HandleMessage(new LocalMessagePackage(line));
				}
			}
		}

		private void GameOver()
		{
			MyPlayer.Disconnect();
		}

		public void TestOver()
		{
			_IsOver = true;
		}
	}
}
