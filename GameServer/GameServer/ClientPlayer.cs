using System;
using System.Collections.Generic;
using System.Net.Sockets;

using GameNetwork;
using Serilog;

namespace GameServer
{
	class ClientPlayer
	{
		public delegate void ClientMessageHandler(byte[] message);

		public ClientAccount Account;
		private Server.ServerMessageHandler _serverMessageHandler;

		private ClientNetwork _network;
		private List<int> _actions;

		private MessageBuffer _messageBuffer;

		public ClientPlayer(TcpClient client, Server.ServerMessageHandler handler)
		{
			_serverMessageHandler = new Server.ServerMessageHandler(handler);

			_network = new ClientNetwork(client, HandleMessage);
			_actions = new List<int>();

			_messageBuffer = new MessageBuffer(null);
		}

		public bool HasInput()
		{
			return _actions.Count > 0;
		}

		public int GetAction()
		{
			int action = _actions[0];
			_actions.Clear();
			return action;
		}

		public void SendGameData(byte[] message)
		{
			if (_network != null)
			{
				_network.Send(message);
			}
		}

		public void Disconnect()
		{
			if (_network != null)
			{
				_network.Stop();
				_network = null;
			}
		}

		public void Update(double deltaTime)
		{
			if (_network != null)
			{
				_network.CheckConnect(deltaTime);

				if (_network.IsConnect)
				{

				}
				else
				{
					Log.Information("斷線");
					_serverMessageHandler(this, BitConverter.GetBytes((int)Message.Disconnect));
				}
			}
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		private void HandleMessage(byte[] message)
		{
			if (message != null)
			{
				_messageBuffer.Reset(message);
				int messageType = _messageBuffer.ReadInt();

				switch (messageType)
				{
					case (int)Message.GameAction:
						int action = _messageBuffer.ReadInt();
						_actions.Add(action);
						break;

					default:
						_serverMessageHandler(this, message);
						break;
				}
			}
			else
			{
				_serverMessageHandler(this, BitConverter.GetBytes((int)Message.Disconnect));
			}
		}
	}
}
