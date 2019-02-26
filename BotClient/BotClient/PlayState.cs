using System;

using Serilog;
using GameNetwork;

namespace BotClient
{
	class PlayState : State<ClientPlayer>
	{
		public void Enter(ClientPlayer player)
		{
			Log.Information("輸入0=石頭，1=剪刀，2=布");
		}

		public void Execute(ClientPlayer player)
		{
			if (!player.Account.IsMatch)
			{
				player.MyStateMachine.ChangeState(new WaitState());
			}
		}

		public void Exit(ClientPlayer player)
		{
			int result = player.Game.GameResult();
			if (result > 0)
			{
				Log.Information("獲勝");
			}
			else if (result < 0)
			{
				Log.Information("失敗");
			}
			else
			{
				Log.Information("平手");
			}

			Log.Information("遊戲結束~目前得分{0}", player.Account.Score.ToString());
			
		}

		public bool HandleMessage(ClientPlayer player, LocalMessagePackage msg)
		{
			if (msg.IsLocal)
			{
				MessageBuffer messageBuffer = new MessageBuffer(new byte[8]);
				int action;
				if (Int32.TryParse(msg.LocalMessage, out action))
				{
					if (action > 2 || action < 0)
					{
						Log.Information("輸入錯誤，請重新輸入");
						return false;
					}

					player.SetAction(action);

					messageBuffer.WriteInt((int)Message.GameAction);
					messageBuffer.WriteInt(action);
					player.SendMessageToServer(messageBuffer.Buffer.Clone() as byte[]);

					return true;
				}
				
			}
			return false;
		}
	}
}
