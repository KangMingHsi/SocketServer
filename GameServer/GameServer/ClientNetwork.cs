using System;
using System.Net.Sockets;
using System.Threading;

using Serilog;
using GameNetwork;

namespace GameServer
{
	class ClientNetwork
	{
		public TcpClient Client { get; }

		public bool CanClose { get { return _writeTask.IsComplete; } }
		public bool IsConnect { get; private set; }

		private ClientReadTask _readTask;
		private ClientWriteTask _writeTask;

		private double _lastUpdateTime = 0.0;
		private double _currentTime = 0.0;
		private double _updateInterval;

		public ClientNetwork(TcpClient client, ClientPlayer.ClientMessageHandler handler)
		{
			Client = client;

			var msgHandle = new ClientPlayer.ClientMessageHandler(handler);
			_readTask = new ClientReadTask(this, msgHandle);
			_writeTask = new ClientWriteTask(this, msgHandle);
			
			ThreadPool.QueueUserWorkItem(StartReadTask, null);
			
			IsConnect = true;
			_updateInterval = Constant.ClientNetworkUpdateInterval;
		}

		public void Stop()
		{
			IsConnect = false;

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

		public void CheckConnect(double deltaTime)
		{
			_currentTime += deltaTime;

			if ((_currentTime - _lastUpdateTime) > _updateInterval)
			{
				_lastUpdateTime = _currentTime;
				Send(BitConverter.GetBytes((int)Message.NoMeaning));
			}
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
