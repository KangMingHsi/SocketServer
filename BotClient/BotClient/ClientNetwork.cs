using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Serilog;
using GameNetwork;

namespace BotClient
{
	class ClientNetwork
	{
		public TcpClient Client { get { return _tcpClient; } }

		private ClientPlayer.MessageHandler _handler;

		private IPAddress _iPAddress;
		private int _port;

		private byte[] _data = new byte[5000];
		private int _bytesRead = 0;

		private TcpClient _tcpClient;
		private ClientReadTask _readTask;
		private ClientWriteTask _writeTask;


		public ClientNetwork(string server, int port, ClientPlayer.MessageHandler handle)
		{
			_iPAddress = IPAddress.Parse(server);
			_port = port;
			_tcpClient = new TcpClient();

			_handler = handle;
		}

		public void Connect()
		{
			try
			{ 
				_tcpClient.BeginConnect(_iPAddress, _port, ConnectCallback, null);
			}
			catch (Exception e)
			{
				Log.Error(e.StackTrace.ToString());
			}
		}

		private void ConnectCallback(IAsyncResult r)
		{
			_tcpClient.EndConnect(r);

			if (_tcpClient.Connected)
			{
				_readTask = new ClientReadTask(this, _handler);
				_writeTask = new ClientWriteTask(this, _handler);
				ThreadPool.QueueUserWorkItem(StartReadTask, _tcpClient);
			}
			else
			{
				Log.Information("連線失敗!");
			}
		}

		public void Stop()
		{
			_readTask.Stop();

			while(!_writeTask.IsComplete)
			{
				Thread.Sleep(1);
			}

			Cleanup();
		}

		public void Close()
		{
			try
			{
				byte[] bytes = BitConverter.GetBytes((int)Message.Disconnect);
				Send(bytes);
			}
			catch (Exception e)
			{
				Log.Information(e.StackTrace.ToString());
			}
			
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
			while (_writeTask == null)
			{
				Thread.Sleep(1);
			}
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
