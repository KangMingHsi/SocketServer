using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace GameServer
{
	class ClientNetwork
	{
		public delegate void MessageHandler(ClientNetwork client, byte[] message);
		public TcpClient Client { get { return _tcpClient; } }

		public bool CanClose { get { return _writeTask.IsComplete; } }

		public ClientAccount Account;

		private TcpClient _tcpClient;
		private ClientReadTask _readTask;
		private ClientWriteTask _writeTask;

		public ClientNetwork(TcpClient client, MessageHandler handler)
		{
			_tcpClient = client;

			_readTask = new ClientReadTask(this, handler);
			_writeTask = new ClientWriteTask(this, handler);

			ThreadPool.QueueUserWorkItem(StartReadTask, client);

		}

		~ClientNetwork()
		{
			Stop();
		}

		public void Stop()
		{
			_readTask.Stop();

			while (!CanClose)
			{
				Thread.Sleep(1);
			}

			Cleanup();
		}

		public void Send(byte[] message)
		{
			ThreadPool.QueueUserWorkItem(StartWriteTask, message);
		}

		private void StartReadTask(object message)
		{
			_readTask.Begin();
		}

		private void StartWriteTask(object message)
		{
			_writeTask.Write(message as byte[]);
		}

		private void Cleanup()
		{
			if (_tcpClient != null)
			{
				_tcpClient.Close();
			}
		}
	}
}
