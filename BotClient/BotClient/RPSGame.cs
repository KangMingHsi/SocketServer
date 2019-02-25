using System;
using System.Collections.Generic;
using System.Threading;

using Serilog;

namespace BotClient
{
	class RPSGame
	{
		public static RPSGame Instance;

		public ClientPlayer MyPlayer { get; private set; }
		public StateMachine<RPSGame> MyStateMachine { get; private set; }

		private static int[,] _winnerLookUpTable = null;

		private bool _IsOver;
		private int _opponentAction;

		public RPSGame()
		{
			if (Instance != null)
			{
				Instance = this;
			}

			InitWinnerLookUpTable();

			_IsOver = false;
		}

		public void InitGame(ClientPlayer myPlayer)
		{
			myPlayer.SetGame(this);
			MyPlayer = myPlayer;

			MyStateMachine = new StateMachine<RPSGame>(this);
			MyStateMachine.SetCurrentState(new LoginState());
		}

		public void GameLoop()
		{
			while (!_IsOver)
			{
				InputHandle();

				MyStateMachine.Update();
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

				Log.Information("Init");
			}
		}

		public int GameResult()
		{
			return _winnerLookUpTable[MyPlayer.GetAction(), _opponentAction];
		}

		private void GameOver()
		{
			MyPlayer.Disconnect();
		}

		private void InputHandle()
		{
			if (Console.KeyAvailable)
			{
				var line = Console.ReadLine();
				
				if (line.Equals("Q", StringComparison.CurrentCultureIgnoreCase))
				{
					_IsOver = true;
				}
				else
				{
					MyStateMachine.HandleMessage(line);
				}
			}
		}
	}
}
