using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace BotClient
{
	class Client
	{
		public static int ConnectCnt;

		IPAddress _iPAddress;
		int _port;

		private byte[] _data = new byte[5000];
		private int _bytesRead = 0;

		TcpClient _tcpClient;
		NetworkStream _stream;

		public Client(string server, int port)
		{
			_iPAddress = IPAddress.Parse(server);
			_port = port;
			_tcpClient = new TcpClient();
			//_tcpClient.Client.Blocking = false;
			
		}

		~Client()
		{
			Cleanup();
		}

		public void Connect()
		{
			try
			{ 
				_tcpClient.BeginConnect(_iPAddress, _port, ConnectCallback, null);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.StackTrace.ToString());
			}
		}

		private void ConnectCallback(IAsyncResult r)
		{
			_tcpClient.EndConnect(r);

			if (_tcpClient.Connected)
			{
				_stream = _tcpClient.GetStream();
				Console.WriteLine("連線成功!");
				ConnectCnt++;

				Console.WriteLine("Current Connect Client: {0}", ConnectCnt.ToString());

				ThreadPool.QueueUserWorkItem(ClientReadBegin, _tcpClient);
			}
			else
			{
				Console.WriteLine("連線失敗!");
			}
		}

		private void ClientReadBegin(object client)
		{
			while (_tcpClient.Connected)
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
					string message = System.Text.Encoding.ASCII.GetString(_data, 0, _bytesRead);
					Console.WriteLine("Get Message {0} from {1} client!", message, _tcpClient.GetHashCode());
					_bytesRead = 0;

					HandleMessage(message);

				}
			}
			catch (Exception ex)
			{
				ProcessException(ex);
			}
		}

		private void HandleMessage(string message)
		{
			if (message.Equals("Bye"))
			{
				Cleanup();
			}
		}

		public void Close()
		{
			Byte[] data = System.Text.Encoding.ASCII.GetBytes("Goodbye");

			try
			{
				_stream.BeginWrite(data, 0, data.Length, WriteCallback, null);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.StackTrace.ToString());
			}
		}

		private void WriteCallback(IAsyncResult r)
		{
			_tcpClient.GetStream().EndWrite(r);

			Cleanup();
			Console.WriteLine("Current Connect Client: {0}", ConnectCnt.ToString());
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
				--ConnectCnt;
			}
			Console.WriteLine("Current Connect Client: {0}", ConnectCnt.ToString());
		}

		private void ProcessException(Exception ex)
		{
			Cleanup();
			Console.WriteLine("Error: " + ex.Message);
		}
	}
}
