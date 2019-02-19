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

			_tcpClient.Close();
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
			}
			else
			{
				Console.WriteLine("連線失敗!");
			}
		}

		public void Close()
		{
			Byte[] data = System.Text.Encoding.ASCII.GetBytes("Goodbye");
			_stream.BeginWrite(data, 0, data.Length, WriteCallback, null);
		}

		private void WriteCallback(IAsyncResult r)
		{
			_tcpClient.GetStream().EndWrite(r);

			_stream.Close();
			_tcpClient.Close();

			ConnectCnt--;
			Console.WriteLine("Current Connect Client: {0}", ConnectCnt.ToString());
		}
	}
}
