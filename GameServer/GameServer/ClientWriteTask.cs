﻿using System;
using System.Net.Sockets;

using Serilog;

namespace GameServer
{
	class ClientWriteTask
	{
		public bool IsComplete { get { return _isComplete; } }
		// TODO check whether this handler is necessary
		private event ClientPlayer.ClientMessageHandler _messageHandler;

		private byte[] _data;

		private ClientNetwork _client;
		private NetworkStream _stream;

		private bool _isComplete;

		public ClientWriteTask(ClientNetwork client, ClientPlayer.ClientMessageHandler handler)
		{
			_client = client;
			_stream = _client.Client.GetStream();
			_messageHandler = new ClientPlayer.ClientMessageHandler(handler);

			_data = null;
			_isComplete = true;
		}

		public void Write(byte[] message)
		{
			_data = message;
			_isComplete = false;
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
				ProcessException(e);
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
				_isComplete = true;
			}
			catch (Exception ex)
			{
				ProcessException(ex);
			}
		}

		private void ProcessException(Exception ex)
		{
			_isComplete = true;
			Log.Error("Error: " + ex.Message);
			_client.Stop();
		}
	}
}
