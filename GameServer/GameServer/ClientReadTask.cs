using System;
using System.Net.Sockets;

namespace GameServer
{
	class ClientReadTask
	{
		private event CustomClient.MessageHandler _messageHandler;

		private readonly byte[] _data = new byte[1024];
		private int _bytesRead = 0;
		private bool _isDisconnect = false;

		private NetworkStream _stream;
		private CustomClient _client;

		public ClientReadTask(CustomClient client, CustomClient.MessageHandler handler)
		{
			_client = client;
			_stream = _client.Client.GetStream();
			_messageHandler = new CustomClient.MessageHandler(handler);
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
					_messageHandler(_client, _data);
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
			_messageHandler(_client, null);
			Console.WriteLine("Error: " + ex.Message);
			_client.Stop();
		}
	}
}
