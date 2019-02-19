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
		
		public void Begin(TcpClient client, Server.DisconnectHandler handler)
		{
			try
			{
				_tcpClient = client;
				_stream = _tcpClient.GetStream();
				
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
			while (!_isDisconnect && _tcpClient.Connected)
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
					string message = System.Text.Encoding.ASCII.GetString(_data, 0, _bytesRead);
					Console.WriteLine("Get Message {0} from {1} client!", message, _tcpClient.GetHashCode());
					_bytesRead = 0;

					// TODO Handle Message we got
					if (string.Compare(message, "Goodbye") == 0)
					{
						_isDisconnect = true;
					}

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
			Console.WriteLine("Error: " + ex.Message);
		}
	}
}
