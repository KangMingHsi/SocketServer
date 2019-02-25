using System;
using System.Net.Sockets;

using Serilog;

namespace BotClient
{
	class ClientWriteTask
	{
		public bool IsComplete { get; private set; }
		
		private event ClientPlayer.MessageHandler _messageHandler;

		private byte[] _data;

		private ClientNetwork _client;
		private NetworkStream _stream;

		public ClientWriteTask(ClientNetwork client, ClientPlayer.MessageHandler handler)
		{
			_client = client;
			_stream = _client.Client.GetStream();
			_messageHandler = new ClientPlayer.MessageHandler(handler);

			_data = null;
			IsComplete = true;
		}

		public void Write(byte[] message)
		{
			try
			{
				_data = message;
				IsComplete = false;
				Write();
			}
			catch
			{
				IsComplete = true;
				throw;
			}
		}

		private void Write()
		{
			_stream.BeginWrite(_data, 0, _data.Length, WriteCallback, null);
		}

		private void WriteCallback(IAsyncResult r)
		{
			try
			{
				_stream.EndWrite(r);
				IsComplete = true;
			}
			catch
			{
				IsComplete = true;
				_messageHandler(null);
			}
		}
	}
}
