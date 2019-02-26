using System.Text;

using Serilog;
using GameNetwork;

namespace BotClient
{
	class GlobalPlayerState : State<ClientPlayer>
	{
		public void Enter(ClientPlayer player){}

		public void Execute(ClientPlayer player){}

		public void Exit(ClientPlayer player) {}

		public bool HandleMessage(ClientPlayer player, LocalMessagePackage msg)
		{
			if (!msg.IsLocal)
			{
				MessageBuffer messageBuffer = new MessageBuffer(msg.NetWorkMessage);
				int messageType = messageBuffer.ReadInt();
				switch (messageType)
				{
					case (int)Message.Disconnect:
						player.Network.Stop();
						break;
					case (int)Message.SignInFail:
						Log.Information("連線失敗!");
						player.Network.Stop();
						break;
					case (int)Message.SignInSuccess:
						player.Account.IsOnline = true;
						player.Account.Score = messageBuffer.ReadInt();
						break;
					case (int)Message.MatchGame:
						if (player.Account.IsMatch)
						{
							Log.Information("平手!重新輸入");
						}
						else
						{
							player.Account.IsMatch = true;
						}
						break;
					case (int)Message.GameAction:
						int opponentAction = messageBuffer.ReadInt();
						player.Game.SetOpponentAction(opponentAction);
						break;
					case (int)Message.GameOver:
						player.Account.Score = messageBuffer.ReadInt();
						player.Account.IsMatch = false;
						break;
					default:
						break;

				}
						return true;
			}

			return false;
		}
	}
}
