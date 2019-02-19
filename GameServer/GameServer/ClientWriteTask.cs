using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace GameServer
{
	class ClientWriteTask
	{
		public event Server.DisconnectHandler DisconnectEvent;

		private readonly byte[] _data = new byte[5000];
		private int _bytesRead = 0;
		private bool _isDisconnect = false;

		private TcpClient _tcpClient;
		private NetworkStream _stream;
		private StringBuilder _processData;

		public void Begin(TcpClient client, Server.DisconnectHandler handler)
		{
			try
			{
				_tcpClient = client;
				_stream = _tcpClient.GetStream();
				DisconnectEvent = new Server.DisconnectHandler(handler);

				Write();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.StackTrace.ToString());
			}

		}

		private void Write()
		{
			Console.WriteLine("Write");
			Array.Reverse(_data);
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

			Cleanup();
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
