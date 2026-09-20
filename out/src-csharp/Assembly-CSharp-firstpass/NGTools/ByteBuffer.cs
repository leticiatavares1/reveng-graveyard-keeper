using System;
using System.Text;

namespace NGTools;

public class ByteBuffer
{
	public enum ResizeMode
	{
		Strict,
		Double
	}

	public ResizeMode resizeMode;

	public readonly bool writable;

	private int length;

	private byte[] buffer;

	public int Capacity => buffer.Length;

	public int Length
	{
		get
		{
			return length;
		}
		set
		{
			if (!writable)
			{
				throw new InvalidOperationException("Buffer is unwritable.");
			}
			length = value;
		}
	}

	public int Position { get; set; }

	public ByteBuffer(int capacity)
	{
		resizeMode = ResizeMode.Double;
		buffer = new byte[capacity];
		writable = true;
	}

	public ByteBuffer(int capacity, ResizeMode mode)
	{
		resizeMode = mode;
		buffer = new byte[capacity];
		writable = true;
	}

	public ByteBuffer(int capacity, bool writable)
	{
		resizeMode = ResizeMode.Double;
		buffer = new byte[capacity];
		this.writable = writable;
	}

	public ByteBuffer(int capacity, ResizeMode mode, bool writable)
	{
		resizeMode = mode;
		buffer = new byte[capacity];
		this.writable = writable;
	}

	public ByteBuffer(byte[] buffer)
	{
		this.buffer = (byte[])buffer.Clone();
		length = this.buffer.Length;
		writable = false;
	}

	public ByteBuffer(byte[] buffer, bool writable)
	{
		this.buffer = (byte[])buffer.Clone();
		length = this.buffer.Length;
		this.writable = writable;
	}

	public void Resize(int newSize)
	{
		Resize(newSize, force: false);
	}

	private void Resize(int newSize, bool force)
	{
		if (!writable && !force)
		{
			return;
		}
		switch (resizeMode)
		{
		case ResizeMode.Strict:
			while (newSize > Length)
			{
				byte[] dst2 = new byte[newSize];
				if (Length > 0)
				{
					Buffer.BlockCopy(buffer, 0, dst2, 0, Length);
				}
				buffer = dst2;
			}
			break;
		case ResizeMode.Double:
		{
			int num = buffer.Length << 1;
			while (newSize > num)
			{
				num <<= 1;
			}
			byte[] dst = new byte[num];
			if (Length > 0)
			{
				Buffer.BlockCopy(buffer, 0, dst, 0, Length);
			}
			buffer = dst;
			break;
		}
		}
	}

	public void AppendUnicodeString(string content)
	{
		if (writable)
		{
			if (string.IsNullOrEmpty(content))
			{
				Append(0);
				return;
			}
			byte[] bytes = Encoding.UTF8.GetBytes(content);
			Append(bytes.Length);
			Append(bytes);
		}
	}

	public void Append(ByteBuffer src)
	{
		if (writable)
		{
			if (Length + src.Length > buffer.Length)
			{
				Resize(Length + src.Length);
			}
			Buffer.BlockCopy(src.buffer, src.Position, buffer, Length, src.Length);
			Length += src.Length;
		}
	}

	public void Append(byte[] src, int position, int length)
	{
		if (writable)
		{
			if (Length + length > buffer.Length)
			{
				Resize(Length + length);
			}
			Buffer.BlockCopy(src, position, buffer, Length, length);
			Length += length;
		}
	}

	public void Append(Array src)
	{
		if (writable)
		{
			if (Length + src.Length > buffer.Length)
			{
				Resize(Length + src.Length);
			}
			Buffer.BlockCopy(src, 0, buffer, Length, src.Length);
			Length += src.Length;
		}
	}

	public void Append(bool value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		Append(bytes);
	}

	public void Append(byte value)
	{
		if (writable)
		{
			if (Length + 1 > buffer.Length)
			{
				Resize(Length + 1);
			}
			buffer[Length] = value;
			Length++;
		}
	}

	public void Append(sbyte value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		Append(bytes);
	}

	public void Append(char value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		Append(bytes);
	}

	public void Append(float value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		Append(bytes);
	}

	public void Append(double value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		Append(bytes);
	}

	public void Append(short value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		Append(bytes);
	}

	public void Append(int value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		Append(bytes);
	}

	public void Append(long value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		Append(bytes);
	}

	public void Append(ushort value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		Append(bytes);
	}

	public void Append(uint value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		Append(bytes);
	}

	public void Append(ulong value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		Append(bytes);
	}

	public void Append(string src)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(src);
		Append(bytes);
	}

	public short ReadInt16()
	{
		if (Position + 2 > Length)
		{
			throw new OverflowException("Unsufficient bytes (" + 2 + " bytes) in buffer of " + Length + " at " + Position + ".");
		}
		short result = BitConverter.ToInt16(buffer, Position);
		Position += 2;
		return result;
	}

	public int ReadInt32()
	{
		if (Position + 4 > Length)
		{
			throw new OverflowException("Unsufficient bytes (" + 4 + " bytes) in buffer of " + Length + " at " + Position + ".");
		}
		int result = BitConverter.ToInt32(buffer, Position);
		Position += 4;
		return result;
	}

	public long ReadInt64()
	{
		if (Position + 8 > Length)
		{
			throw new OverflowException("Unsufficient bytes (" + 8 + " bytes) in buffer of " + Length + " at " + Position + ".");
		}
		long result = BitConverter.ToInt64(buffer, Position);
		Position += 8;
		return result;
	}

	public ushort ReadUInt16()
	{
		if (Position + 2 > Length)
		{
			throw new OverflowException("Unsufficient bytes (" + 2 + " bytes) in buffer of " + Length + " at " + Position + ".");
		}
		ushort result = BitConverter.ToUInt16(buffer, Position);
		Position += 2;
		return result;
	}

	public uint ReadUInt32()
	{
		if (Position + 4 > Length)
		{
			throw new OverflowException("Unsufficient bytes (" + 4 + " bytes) in buffer of " + Length + " at " + Position + ".");
		}
		uint result = BitConverter.ToUInt32(buffer, Position);
		Position += 4;
		return result;
	}

	public ulong ReadUInt64()
	{
		if (Position + 8 > Length)
		{
			throw new OverflowException("Unsufficient bytes (" + 8 + " bytes) in buffer of " + Length + " at " + Position + ".");
		}
		ulong result = BitConverter.ToUInt64(buffer, Position);
		Position += 8;
		return result;
	}

	public float ReadSingle()
	{
		if (Position + 4 > Length)
		{
			throw new OverflowException("Unsufficient bytes (" + 2 + " bytes) in buffer of " + Length + " at " + Position + ".");
		}
		float result = BitConverter.ToSingle(buffer, Position);
		Position += 4;
		return result;
	}

	public double ReadDouble()
	{
		if (Position + 8 > Length)
		{
			throw new OverflowException("Unsufficient bytes (" + 8 + " bytes) in buffer of " + Length + " at " + Position + ".");
		}
		double result = BitConverter.ToDouble(buffer, Position);
		Position += 8;
		return result;
	}

	public byte ReadByte()
	{
		if (Position + 1 > Length)
		{
			throw new OverflowException("Unsufficient bytes (" + 1 + " bytes) in buffer of " + Length + " at " + Position + ".");
		}
		Position++;
		return buffer[Position - 1];
	}

	public sbyte ReadSByte()
	{
		if (Position + 1 > Length)
		{
			throw new OverflowException("Unsufficient bytes (" + 1 + " bytes) in buffer of " + Length + " at " + Position + ".");
		}
		Position++;
		return (sbyte)buffer[Position - 1];
	}

	public bool ReadBoolean()
	{
		if (Position + 1 > Length)
		{
			throw new OverflowException("Unsufficient bytes (" + 1 + " bytes) in buffer of " + Length + " at " + Position + ".");
		}
		bool result = BitConverter.ToBoolean(buffer, Position);
		Position++;
		return result;
	}

	public char ReadChar()
	{
		if (Position + 2 > Length)
		{
			throw new OverflowException("Unsufficient bytes (" + 2 + " bytes) in buffer of " + Length + " at " + Position + ".");
		}
		char result = BitConverter.ToChar(buffer, Position);
		Position += 2;
		return result;
	}

	public string ReadString(int length)
	{
		if (Position + length > Length)
		{
			throw new OverflowException("Unsufficient bytes (" + length + " bytes) in buffer of " + Length + " at " + Position + ".");
		}
		string @string = Encoding.UTF8.GetString(buffer, Position, length);
		Position += length;
		return @string;
	}

	public string ReadUnicodeString()
	{
		int num = ReadInt32();
		if (num > 0)
		{
			return Encoding.UTF8.GetString(ReadBytes(num));
		}
		return string.Empty;
	}

	public byte[] ReadBytes(int length)
	{
		if (Position + length > Length)
		{
			throw new OverflowException("Unsufficient bytes (" + length + " bytes) in buffer of " + Length + " at " + Position + ".");
		}
		byte[] array = new byte[length];
		Buffer.BlockCopy(buffer, Position, array, 0, length);
		Position += length;
		return array;
	}

	public void Clear()
	{
		Length = 0;
		Position = 0;
	}

	public byte[] Flush()
	{
		byte[] result = GetBuffer();
		Clear();
		return result;
	}

	public byte[] GetRawBuffer()
	{
		return buffer;
	}

	public byte[] GetBuffer()
	{
		byte[] array = new byte[Length];
		Buffer.BlockCopy(buffer, 0, array, 0, Length);
		return array;
	}

	public void CopyBuffer(ByteBuffer destination, int length)
	{
		if (Position + length > Length)
		{
			throw new OverflowException("Unsufficient bytes in buffer of " + Length + " at " + Position + ".");
		}
		if (destination.buffer.Length < length)
		{
			destination.Resize(length, force: true);
		}
		Buffer.BlockCopy(buffer, Position, destination.buffer, 0, length);
		destination.length = length;
		destination.Position = 0;
	}

	public void CopyBuffer(ByteBuffer destination, int position, int length)
	{
		if (position + length > Length)
		{
			throw new OverflowException("Unsufficient bytes in buffer of " + Length + " at " + Position + ".");
		}
		if (destination.buffer.Length < length)
		{
			destination.Resize(length, force: true);
		}
		Buffer.BlockCopy(buffer, position, destination.buffer, 0, length);
		destination.length = length;
		destination.Position = 0;
	}
}
