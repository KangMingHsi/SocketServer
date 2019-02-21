using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace GameServer
{
	class ClientWriteTask
	{
		// TODO check whether this handler is necessary
		private event CustomClient.MessageHandler _messageHandler;

		private byte[] _data;

		private CustomClient _client;
		private NetworkStream _stream;

		public ClientWriteTask(CustomClient client, CustomClient.MessageHandler handler)
		{
			_client = client;
			_stream = _client.Client.GetStream();
			_messageHandler = new CustomClient.MessageHandler(handler);
			_data = null;
		}

		public void Write(byte[] message)
		{
			_data = message;
			Begin();
		}

		private void Begin()
		{
			try
			{
				Write();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.StackTrace.ToString());
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
			}
			catch (Exception ex)
			{
				ProcessException(ex);
			}
		}

		private void ProcessException(Exception ex)
		{
			_messageHandler(_client, null);
			Console.WriteLine("Error: " + ex.Message);
			_client.Stop();
		}
	}
}
