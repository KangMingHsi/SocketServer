using System;
using System.Collections.Generic;
using System.Threading;

using Serilog;
using GameNetwork;

namespace BotClient
{
	class ClientPlayer
	{
		public delegate void MessageHandler(byte[] message);
		
		public RPSGame Game;
		public ClientAccount Account;

		public StateMachine<ClientPlayer> MyStateMachine { get; private set; }
		public ClientNetwork Network { get; private set; }

		private MessageBuffer _messageBuffer;
		private Random _decisionPolicy;
		private int _action;

		public ClientPlayer(string configPath)
		{
			string[] config = Config.ReadPlayerConfig(configPath);

			Network = new ClientNetwork(config[0], Int32.Parse(config[1]), MessageHandle);
			_decisionPolicy = new Random();
			_messageBuffer = new MessageBuffer(null);

			MyStateMachine = new StateMachine<ClientPlayer>(this);
			MyStateMachine.SetCurrentState(new LoginState());
			MyStateMachine.SetGlobalState(new GlobalPlayerState());
		}

		public int GetAction()
		{
			return _action;
		}

		public void SetAction(int action)
		{
			_action = action;
		}

		public void SetAction()
		{
			_action = _decisionPolicy.Next(0, 3);
		}

		public void Update()
		{
			MyStateMachine.Update();
		}

		public void ConnectToServer()
		{
			Network.Connect();
			SendMessageToServer(ClientAccount.ToBytes(Account));
		}

		public void SendMessageToServer(byte[] message)
		{
			Network.Send(message);
		}

		public void Disconnect()
		{
			Network.Close();
			Thread.Sleep(3);
			Network.Stop();
		}

		private void MessageHandle(byte[] message)
		{
			if (message != null)
			{
				MyStateMachine.HandleMessage(new LocalMessagePackage(message));
				//_messageBuffer.Reset(message);
				//int messageType = _messageBuffer.ReadInt();
				//switch (messageType)
				//{
				//	case (int)Message.Disconnect:
				//		Network.Stop();
				//		break;
				//	case (int)Message.SignInFail:
				//		Log.Information("連線失敗!");
				//		Network.Stop();
				//		break;
				//	case (int)Message.SignInSuccess:
				//		Account.IsOnline = true;
				//		Account.Score = _messageBuffer.ReadInt();
				//		break;
				//	case (int)Message.MatchGame:
				//		if (Account.IsMatch)
				//		{
				//			Log.Information("平手!重新輸入");
				//		}
				//		else
				//		{
				//			Account.IsMatch = true;
				//		}
				//		break;
				//	case (int)Message.GameAction:
				//		int opponentAction = _messageBuffer.ReadInt();
				//		Game.SetOpponentAction(opponentAction);
				//		break;
				//	case (int)Message.GameOver:
				//		Account.Score = _messageBuffer.ReadInt();
				//		Account.IsMatch = false;
				//		break;
				//	default:
				//		break;
				//}
			}
			else
			{
				// write task error
			}
		}
	}
}
