using System;
using System.Threading;
using System.Net.Sockets;

using Serilog;

namespace GameServer
{
	class ClientReadTask
	{
		private event ClientPlayer.ClientMessageHandler _messageHandler;

		private readonly byte[] _data = new byte[1024];
		private int _bytesRead = 0;
		private bool _isDisconnect = false;

		private NetworkStream _stream;
		private ClientNetwork _client;

		public ClientReadTask(ClientNetwork client, ClientPlayer.ClientMessageHandler handler)
		{
			_client = client;
			_stream = _client.Client.GetStream();
			_messageHandler = new ClientPlayer.ClientMessageHandler(handler);
		}

		public void Stop()
		{
			_isDisconnect = true;
		}

		public void Begin()
		{
			try
			{
				Read();
			}
			catch (Exception e)
			{
				ProcessException(e);
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

				Thread.Sleep(1);
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
					_messageHandler(_data);
				}
			}
			catch (Exception ex)
			{
				ProcessException(ex);
			}
		}

		private void ProcessException(Exception ex)
		{
			// TODO define error code;
			_messageHandler(null);
			Log.Error("Error: " + ex.Message);
			_client.Stop();
		}
	}
}
