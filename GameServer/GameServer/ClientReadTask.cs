using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace GameServer
{
	class ClientReadTask : ClientTask
	{
		private event Server.MessageHandler _messageHandler;

		private readonly byte[] _data = new byte[1024];
		private int _bytesRead = 0;
		private bool _isDisconnect = false;

		private TcpClient _tcpClient;
		private NetworkStream _stream;

		public ClientReadTask(TcpClient client, Server.MessageHandler handler)
		{
			_tcpClient = client;
			_stream = _tcpClient.GetStream();
			_messageHandler = new Server.MessageHandler(handler);
		}

		~ClientReadTask()
		{
			Cleanup();
		}

		public TcpClient GetTcpClient()
		{
			return _tcpClient;
		}

		public void Stop()
		{
			_isDisconnect = true;
			Cleanup();
		}

		public void Begin()
		{
			try
			{
				Read();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.StackTrace.ToString());
			}
		}

		private void Read()
		{
			// Read in a nonblocking function.
			while (!_isDisconnect)
			{
				if (_stream.DataAvailable)
				{
					IAsyncResult r = _stream.BeginRead(_data, _bytesRead, _data.Length - _bytesRead, ReadCallback, null);
				}
			}
		}

		private void ReadCallback(IAsyncResult r)
		{
			try
			{
				int chunkSize = _stream.EndRead(r);
				_bytesRead += chunkSize;

				if (_stream.DataAvailable)
				{
					_stream.BeginRead(_data, _bytesRead, _data.Length - _bytesRead, ReadCallback, null);
				}
				else
				{
					_bytesRead = 0;
					_messageHandler(this, _data);
				}
			}
			catch (Exception ex)
			{
				ProcessException(ex);
			}
		}

		private void Cleanup()
		{
			if (_stream != null)
			{
				_stream.Close();
			}

			if (_tcpClient != null)
			{
				_tcpClient.Close();
			}
		}

		private void ProcessException(Exception ex)
		{
			Cleanup();

			// TODO define error code;
			_messageHandler(this, null);
			Console.WriteLine("Error: " + ex.Message);
		}
	}
}
