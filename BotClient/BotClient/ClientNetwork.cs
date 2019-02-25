using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Serilog;
using GameNetwork;

namespace BotClient
{
	class ClientNetwork
	{
		public TcpClient Client { get; private set; }
		public bool IsConnect { get; private set; }

		private ClientPlayer.MessageHandler _handler;

		private IPAddress _iPAddress;
		private int _port;

		private ClientReadTask _readTask = null;
		private ClientWriteTask _writeTask = null;

		public ClientNetwork(string server, int port, ClientPlayer.MessageHandler handle)
		{
			_iPAddress = IPAddress.Parse(server);
			_port = port;

			_handler = handle;
			IsConnect = false;
		}

		public void Connect()
		{
			try
			{
				Client = new TcpClient();
				Client.BeginConnect(_iPAddress, _port, ConnectCallback, null);
			}
			catch (Exception e)
			{
				ProcessException(e);
			}
		}

		private void ConnectCallback(IAsyncResult r)
		{
			Client.EndConnect(r);

			if (Client.Connected)
			{
				IsConnect = true;

				_readTask = new ClientReadTask(this, _handler);
				_writeTask = new ClientWriteTask(this, _handler);
				ThreadPool.QueueUserWorkItem(StartReadTask, Client);
			}
			else
			{
				Log.Information("連線失敗!");
			}
		}

		public void Fail()
		{
			IsConnect = false;
		}

		public void Stop()
		{
			IsConnect = false;

			while (!_writeTask.IsComplete)
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
				ProcessException(e);
			}
			
		}

		public void Send(byte[] message)
		{
			ThreadPool.QueueUserWorkItem(StartWriteTask, message);
		}

		private void StartReadTask(object message)
		{
			try
			{
				_readTask.Read();
			}
			catch (SocketException e)
			{
				if (e.NativeErrorCode.Equals(10035))
				{
					IsConnect = true;
				}
				else
				{
					ProcessException(e);
				}
			}
			catch (Exception e)
			{
				ProcessException(e);
			}
		}

		private void StartWriteTask(object message)
		{
			while (_writeTask == null)
			{
				Thread.Sleep(1);
			}

			try
			{
				_writeTask.Write(message as byte[]);
			}
			catch (SocketException e)
			{
				if (e.NativeErrorCode.Equals(10035))
				{
					IsConnect = true;
				}
				else
				{
					ProcessException(e);
				}
			}
			catch (Exception e)
			{
				ProcessException(e);
			}
		}

		private void Cleanup()
		{
			if (Client != null)
			{
				Client.Close();
			}
		}

		private void ProcessException(Exception ex)
		{
			Log.Error("Error: " + ex.Message);
			Log.Error("~Disconnect~");
			Stop();
		}
	}

	
}
