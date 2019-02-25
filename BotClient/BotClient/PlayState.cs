using System;

using Serilog;
using GameNetwork;

namespace BotClient
{
	class PlayState : State<RPSGame>
	{
		public void Enter(RPSGame game)
		{
			Log.Information("輸入0=石頭，1=剪刀，2=布");
		}

		public void Execute(RPSGame game)
		{
			if (!game.MyPlayer.Account.IsMatch)
			{
				game.MyStateMachine.ChangeState(new WaitState());
			}
		}

		public void Exit(RPSGame game)
		{
			int result = game.GameResult();
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
				Log.Information("斷線");
			}

			Log.Information("遊戲結束~目前得分{0}", game.MyPlayer.Account.Score.ToString());
			
		}

		public bool HandleMessage(RPSGame game, string msg)
		{
			MessageBuffer messageBuffer = new MessageBuffer(new byte[8]);
			int action;
			if (Int32.TryParse(msg, out action))
			{
				if (action > 2 || action < 0)
				{
					Log.Information("輸入錯誤，請重新輸入");
					return false;
				}

				game.MyPlayer.SetAction(action);

				messageBuffer.WriteInt((int)Message.GameAction);
				messageBuffer.WriteInt(action);
				game.MyPlayer.SendMessageToServer(messageBuffer.Buffer.Clone() as byte[]);

				return true;
			}
			else
			{
				game.MyStateMachine.ChangeState(new WaitState());
			}

			return false;
		}
	}
}
