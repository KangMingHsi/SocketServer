using System;

using Serilog;

namespace BotClient
{
	class ReadyState : State<RPSGame>
	{
		public void Enter(RPSGame game)
		{
			Log.Information("等待列隊中");
		}

		public void Execute(RPSGame game)
		{
			if (game.MyPlayer.Account.IsMatch)
			{
				game.MyStateMachine.ChangeState(new PlayState());
			}
		}

		public void Exit(RPSGame game)
		{
			Log.Information("配對成功");
			Log.Information("開始遊戲");
		}

		public bool HandleMessage(RPSGame game, string msg)
		{
			return false;
		}
	}
}
