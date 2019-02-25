using System;

using Serilog;

namespace BotClient
{
	class LoginState : State<RPSGame>
	{
		public void Enter(RPSGame game)
		{
			
		}

		public void Execute(RPSGame game)
		{
			if (game.MyPlayer.Account.IsOnline)
			{
				game.MyStateMachine.ChangeState(new WaitState());
			}
		}

		public void Exit(RPSGame game)
		{
			Log.Information("登入成功");
		}

		public bool HandleMessage(RPSGame game, string msg)
		{
			if (game.MyPlayer.Account.Username != null && game.MyPlayer.Account.Password != null)
			{
				game.MyPlayer.Account.Username = null;
				game.MyPlayer.Account.Password = null;
			}

			if (game.MyPlayer.Account.Username == null)
			{
				game.MyPlayer.Account.Username = msg;
			}
			else
			{
				game.MyPlayer.Account.Password = msg;
				game.MyPlayer.ConnectToServer();
			}

			return true;
		}
	}
}
