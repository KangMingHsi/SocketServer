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

		private ClientNetwork _network;
		private MessageBuffer _messageBuffer;
		private Random _decisionPolicy;

		public ClientPlayer()
		{
			_network = new ClientNetwork("127.0.0.1", 36000, MessageHandle);
			_decisionPolicy = new Random();
			_messageBuffer = new MessageBuffer(null);
		}

		public void ConnectToServer()
		{
			_network.Connect();
			Thread.Sleep(1);
		}

		public void SendMessageToServer(byte[] message)
		{
			_network.Send(message);
			Thread.Sleep(1);
		}

		public void Disconnect()
		{
			_network.Close();
			Thread.Sleep(1);
			_network.Stop();
		}

		public int GetAction()
		{
			return _decisionPolicy.Next(0,3);
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
					case (int)Message.SignInSuccess:
						Log.Information("連線成功!");
						break;
					case (int)Message.SignInFail:
						Log.Information("連線失敗!");
						_network.Stop();
						break;

					case (int)Message.MatchGame:
						// TODO match data...
						IsGaming = true;

						Log.Information("Game Start");
						_messageBuffer.Reset(new byte[8]);

						MyAction = GetAction();
						_messageBuffer.WriteInt((int)Message.GameAction);
						_messageBuffer.WriteInt(MyAction);
						_network.Send(_messageBuffer.Buffer.Clone() as byte[]);

						break;
					case (int)Message.GameAction:
						int opponentAction = _messageBuffer.ReadInt();
						bool win = Game.GameResult(MyAction, opponentAction) > 0;

						Log.Information("Win:{0}", win.ToString());
						break;
					case (int)Message.GameOver:
						Log.Information("GameOver!");
						IsGaming = false;
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

		// TODO test
		public RPSGame Game;
		public bool IsGaming;
		int MyAction;
		public void TestGame()
		{
			IsGaming = false;
			Game = new RPSGame();
		}
	}
}
