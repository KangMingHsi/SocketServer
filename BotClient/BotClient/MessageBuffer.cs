using System;
using System.Text;

namespace GameNetwork
{
	public class MessageBuffer
	{
		public byte[] Buffer { get; private set; }
		public int Size { get { return Buffer.Length; } }
		public int Position { get; set; } = 0;

		public MessageBuffer(byte[] buffer)
		{
			Buffer = buffer;
		}

		public void Reset()
		{
			Position = 0;
		}

		public void Reset(byte[] buffer)
		{
			Buffer = buffer;
			Position = 0;
		}

		public byte ReadByte()
		{
			var value = Buffer[Position];
			++Position;

			return value;
		}

		public byte[] ReadBytes(int cnt)
		{
			byte[] value = new byte[cnt];

			for (int i = 0; i < cnt; ++i)
			{
				value[i] = Buffer[Position];
				++Position;
			}

			return value;
		}

		public sbyte ReadSByte()
		{
			var value = (sbyte)Buffer[Position];
			Position++;

			return value;
		}

		public ushort ReadUShort()
		{
			var value = BitConverter.ToUInt16(Buffer, Position);
			Position += 2;

			return value;
		}

		public short ReadShort()
		{
			var value = BitConverter.ToInt16(Buffer, Position);
			Position += 2;

			return value;
		}

		public uint ReadUInt()
		{
			var value = BitConverter.ToUInt32(Buffer, Position);
			Position += 4;

			return value;
		}

		public int ReadInt()
		{
			var value = BitConverter.ToInt32(Buffer, Position);
			Position += 4;

			return value;
		}

		public float ReadFloat()
		{
			var value = BitConverter.ToSingle(Buffer, Position);
			Position += 4;

			return value;
		}

		public double ReadDouble()
		{
			var value = BitConverter.ToDouble(Buffer, Position);
			Position += 8;

			return value;
		}

		public bool ReadBoolean()
		{
			var value = BitConverter.ToBoolean(Buffer, Position);
			Position++;

			return value;
		}

		public string ReadString()
		{
			int stringLen = ReadInt();
			var value = Encoding.UTF8.GetString(Buffer, Position, stringLen);

			//int stringEnd = Array.IndexOf(Buffer, (byte)'\0', Position) + 1;
			//var value = Encoding.UTF8.GetString(Buffer, Position, stringEnd - Position);
			Position += value.Length;

			return value;
		}

		public void WriteByte(byte value)
		{
			Buffer[Position] = value;
			Position++;
		}

		public void WriteSByte(sbyte value)
		{
			Buffer[Position] = (byte)value;
			Position++;
		}

		public void WriteUShort(ushort value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			Array.Copy(bytes, 0, Buffer, Position, 2);
			Position += 2;
		}

		public void WriteShort(short value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			Array.Copy(bytes, 0, Buffer, Position, 2);
			Position += 2;
		}

		public void WriteUInt(uint value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			Array.Copy(bytes, 0, Buffer, Position, 4);
			Position += 4;
		}

		public void WriteInt(int value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			Array.Copy(bytes, 0, Buffer, Position, 4);
			Position += 4;
		}

		public void WriteFloat(float value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			Array.Copy(bytes, 0, Buffer, Position, 4);
			Position += 4;
		}

		public void WriteDouble(double value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			Array.Copy(bytes, 0, Buffer, Position, 8);
			Position += 8;
		}

		public void WriteBoolean(bool value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			Array.Copy(bytes, 0, Buffer, Position, 1);
			Position += 1;
		}

		public void WriteString(string value)
		{
			WriteInt(value.Length);
			
			byte[] bytes = Encoding.UTF8.GetBytes(value);
			Array.Copy(bytes, 0, Buffer, Position, bytes.Length);
			Position += bytes.Length;
		}
	}
}