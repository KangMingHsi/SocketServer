using System;

using Serilog;

namespace BotClient
{
	class ReadyState : State<ClientPlayer>
	{
		public void Enter(ClientPlayer player)
		{
			Log.Information("等待列隊中");
		}

		public void Execute(ClientPlayer player)
		{
			if (player.Account.IsMatch)
			{
				player.MyStateMachine.ChangeState(new PlayState());
			}
		}

		public void Exit(ClientPlayer player)
		{
			Log.Information("配對成功");
			Log.Information("開始遊戲");
		}

		public bool HandleMessage(ClientPlayer player, LocalMessagePackage msg)
		{
			return false;
		}
	}
}
