using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Serilog;
using GameNetwork;

namespace BotClient
{
	class CustomClient
	{
		public delegate void MessageHandler(byte[] message);

		public static int ConnectCnt;
		public TcpClient Client { get { return _tcpClient; } }


		IPAddress _iPAddress;
		int _port;

		private byte[] _data = new byte[5000];
		private int _bytesRead = 0;

		private TcpClient _tcpClient;
		private ClientReadTask _readTask;
		private ClientWriteTask _writeTask;

		public CustomClient(string server, int port)
		{
			_iPAddress = IPAddress.Parse(server);
			_port = port;
			_tcpClient = new TcpClient();
		}

		~CustomClient()
		{
			Stop();
		}

		public void Connect()
		{
			try
			{ 
				_tcpClient.BeginConnect(_iPAddress, _port, ConnectCallback, null);
			}
			catch (Exception e)
			{
				Log.Information(e.StackTrace.ToString());
			}
		}

		private void ConnectCallback(IAsyncResult r)
		{
			_tcpClient.EndConnect(r);

			if (_tcpClient.Connected)
			{
				_readTask = new ClientReadTask(this, HandleMessage);
				_writeTask = new ClientWriteTask(this, HandleMessage);
				ConnectCnt++;

				Log.Information("連線成功!");
				Log.Information("Current Connect Client: {0}", ConnectCnt.ToString());

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
			byte[] message = new byte[10];
			MessageBuffer messageBuffer = new MessageBuffer(message);

			try
			{
				messageBuffer.WriteInt(Message.Disconnect);
				Send(messageBuffer.Buffer);
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

		private void HandleMessage(byte[] message)
		{
			MessageBuffer messageBuffer = new MessageBuffer(message);
			int messageType = messageBuffer.ReadInt();
			switch (messageType)
			{
				case Message.Disconnect:
					
					Stop();
					break;
				case Message.SignInSuccess:
					Log.Information("Success");
					break;
				case Message.SignInFail:
					Log.Information("Fail");
					break;
				default:
					break;
			}
		}

		private void Cleanup()
		{
			if (_tcpClient != null)
			{
				_tcpClient.Close();
				--ConnectCnt;
			}
			Log.Information("Current Connect Client: {0}", ConnectCnt.ToString());
		}
	}

	
}
