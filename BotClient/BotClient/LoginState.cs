using System;

using Serilog;

namespace BotClient
{
	class LoginState : State<ClientPlayer>
	{
		public void Enter(ClientPlayer player)
		{
			Log.Information("請輸入帳號密碼");
		}

		public void Execute(ClientPlayer player)
		{
			if (player.Account.IsOnline)
			{
				player.MyStateMachine.ChangeState(new WaitState());
			}
		}

		public void Exit(ClientPlayer player)
		{
			Log.Information("登入成功");
		}

		public bool HandleMessage(ClientPlayer player, LocalMessagePackage msg)
		{
			if (msg.IsLocal)
			{
				if (player.Account.Username != null && player.Account.Password != null)
				{
					player.Account.Username = null;
					player.Account.Password = null;
				}

				if (player.Account.Username == null)
				{
					player.Account.Username = msg.LocalMessage;
				}
				else
				{
					player.Account.Password = msg.LocalMessage;
					player.ConnectToServer();
				}

				return true;
			}

			return false;
		}
	}
}
