using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace GameServer
{
	class ClientReadTask
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
				_processData = new StringBuilder();

				DisconnectEvent = new Server.DisconnectHandler(handler);
				
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
			
			DisconnectEvent(_tcpClient);
			Cleanup();
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
					_processData.Clear();
					_processData.Append(System.Text.Encoding.ASCII.GetString(_data, 0, _bytesRead));
					Console.WriteLine("Get {0}!", _processData.ToString());
					_bytesRead = 0;

					if (string.Compare(_processData.ToString(), "Goodbye") == 0)
					{
						_isDisconnect = true;
					}

					//ThreadPool.QueueUserWorkItem(ClientWriteBegin, _tcpClient);
				}
			}
			catch (Exception ex)
			{
				ProcessException(ex);
			}
		}

		private void ClientWriteBegin(object client)
		{
			ClientWriteTask task = new ClientWriteTask();
			task.Begin(client as TcpClient, DisconnectEvent);
			//Console.WriteLine("Remain Connections: {0}", _curClient.ToString());
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
