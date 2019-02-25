using System;

using Serilog;
using GameNetwork;

namespace BotClient
{
	class WaitState : State<RPSGame>
	{
		public void Enter(RPSGame game)
		{
			Log.Information("輸入按鍵S開始列隊");
		}

		public void Execute(RPSGame game) {}

		public void Exit(RPSGame game) {}

		public bool HandleMessage(RPSGame game, string msg)
		{
			if (msg.Equals("S", StringComparison.CurrentCultureIgnoreCase))
			{
				game.MyPlayer.SendMessageToServer(BitConverter.GetBytes((int)Message.MatchGame));
				game.MyStateMachine.ChangeState(new ReadyState());
				return true;
			}

			return false;
		}
	}
}
