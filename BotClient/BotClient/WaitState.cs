using System;

using Serilog;
using GameNetwork;

namespace BotClient
{
	class WaitState : State<ClientPlayer>
	{
		public void Enter(ClientPlayer player)
		{
			Log.Information("輸入按鍵S開始列隊");
		}

		public void Execute(ClientPlayer player) {}

		public void Exit(ClientPlayer player) {}

		public bool HandleMessage(ClientPlayer player, LocalMessagePackage msg)
		{
			if (msg.IsLocal)
			{
				if (msg.LocalMessage.Equals("S", StringComparison.CurrentCultureIgnoreCase))
				{
					player.SendMessageToServer(BitConverter.GetBytes((int)Message.MatchGame));
					player.MyStateMachine.ChangeState(new ReadyState());
					return true;
				}
			}
			return false;
		}
	}
}
