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

		public ClientAccount Account;

		private RPSGame _game;
		private ClientNetwork _network;
		private MessageBuffer _messageBuffer;

		private Random _decisionPolicy;
		private int _action;

		public ClientPlayer(string configPath)
		{
			string[] config = Config.ReadPlayerConfig(configPath);

			_network = new ClientNetwork(config[0], Int32.Parse(config[1]), MessageHandle);
			_decisionPolicy = new Random();
			_messageBuffer = new MessageBuffer(null);
		}

		public void SetGame(RPSGame game)
		{
			_game = game;
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

		public void ConnectToServer()
		{
			_network.Connect();

			byte[] accountData = ClientAccount.ToBytes(Account);
			SendMessageToServer(accountData);
		}

		public void SendMessageToServer(byte[] message)
		{
			_network.Send(message);
		}

		public void Disconnect()
		{
			_network.Close();
			Thread.Sleep(3);
			_network.Stop();
		}

		private void MessageHandle(byte[] message)
		{
			if (message != null)
			{
				_messageBuffer.Reset(message);
				int messageType = _messageBuffer.ReadInt();
				switch (messageType)
				{
					case (int)Message.Disconnect:
						_network.Stop();
						break;
					case (int)Message.SignInFail:
						Log.Information("連線失敗!");
						_network.Stop();
						break;
					case (int)Message.SignInSuccess:
						Account.IsOnline = true;
						Account.Score = _messageBuffer.ReadInt();
						break;
					case (int)Message.MatchGame:
						if (Account.IsMatch)
						{
							Log.Information("平手!重新輸入");
						}
						else
						{
							Account.IsMatch = true;
						}
						break;
					case (int)Message.GameAction:
						int opponentAction = _messageBuffer.ReadInt();
						_game.SetOpponentAction(opponentAction);
						break;
					case (int)Message.GameOver:
						Account.Score = _messageBuffer.ReadInt();
						Account.IsMatch = false;
						break;
					default:
						break;
				}
			}
			else
			{
				// write task error
			}
		}
	}
}
