using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace GameServer
{
	class ClientWriteTask : ClientTask
	{
		// TODO check whether this handler is necessary
		private event Server.MessageHandler _messageHandler;

		private byte[] _data;

		private TcpClient _tcpClient;
		private NetworkStream _stream;

		public ClientWriteTask(TcpClient client, byte[] message, Server.MessageHandler handler)
		{
			_tcpClient = client;
			_stream = _tcpClient.GetStream();
			_messageHandler = new Server.MessageHandler(handler);
			_data = message;
		}

		~ClientWriteTask()
		{
			Cleanup();
		}

		public TcpClient GetTcpClient()
		{
			return _tcpClient;
		}

		public void Stop()
		{
			Cleanup();
		}

		public void Begin()
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
			Console.WriteLine("Error: " + ex.Message);
		}
	}
}
