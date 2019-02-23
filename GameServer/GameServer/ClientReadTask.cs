using System;
using System.Threading;
using System.Net.Sockets;

using Serilog;

namespace GameServer
{
	class ClientReadTask
	{
		private event ClientPlayer.ClientMessageHandler _messageHandler;

		private byte[] _data = new byte[2048];
		private int _bytesRead = 0;
		
		private NetworkStream _stream;
		private ClientNetwork _client;

		public ClientReadTask(ClientNetwork client, ClientPlayer.ClientMessageHandler handler)
		{
			_client = client;
			_stream = _client.Client.GetStream();
			_messageHandler = handler;
		}

		public void Read()
		{
			while (_client.IsConnect)
			{
				try
				{
					if (_stream.DataAvailable)
					{
						IAsyncResult r = _stream.BeginRead(_data, _bytesRead, _data.Length - _bytesRead, ReadCallback, null);
					}

					Thread.Sleep(1);
				}
				catch
				{
					throw;
				}
			}
		}

		private void ReadCallback(IAsyncResult r)
		{
			try
			{
				int chunkSize = _stream.EndRead(r);
				_bytesRead += chunkSize;

				if (!_stream.DataAvailable)
				{
					_bytesRead = 0;
					_messageHandler(_data);
				}
				else
				{
					var b = (new byte[_data.Length * 2]);

					_data.CopyTo(b, 0);
					_data = b;
				}
				
			}
			catch
			{
				_messageHandler(null);
			}
		}
	}
}
