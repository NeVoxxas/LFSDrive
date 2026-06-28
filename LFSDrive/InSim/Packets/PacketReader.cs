using System.Buffers.Binary;
using System.Text;

namespace LfsCruise.InSim.Packets;

public sealed class PacketReader
{
    private readonly byte[] _data;

    public PacketReader(byte[] data)
    {
        _data = data;
    }

    public int Position { get; private set; }

    public byte ReadByte()
    {
        EnsureAvailable(sizeof(byte));

        return _data[Position++];
    }

    public sbyte ReadSByte()
    {
        return unchecked((sbyte)ReadByte());
    }

    public ushort ReadUInt16()
    {
        EnsureAvailable(sizeof(ushort));

        var value = BinaryPrimitives.ReadUInt16LittleEndian(_data.AsSpan(Position, sizeof(ushort)));
        Position += sizeof(ushort);

        return value;
    }

    public ushort ReadUShort()
    {
        return ReadUInt16();
    }

    public short ReadInt16()
    {
        EnsureAvailable(sizeof(short));

        var value = BinaryPrimitives.ReadInt16LittleEndian(_data.AsSpan(Position, sizeof(short)));
        Position += sizeof(short);

        return value;
    }

    public short ReadShort()
    {
        return ReadInt16();
    }

    public uint ReadUInt32()
    {
        EnsureAvailable(sizeof(uint));

        var value = BinaryPrimitives.ReadUInt32LittleEndian(_data.AsSpan(Position, sizeof(uint)));
        Position += sizeof(uint);

        return value;
    }

    public uint ReadUInt()
    {
        return ReadUInt32();
    }

    public int ReadInt32()
    {
        EnsureAvailable(sizeof(int));

        var value = BinaryPrimitives.ReadInt32LittleEndian(_data.AsSpan(Position, sizeof(int)));
        Position += sizeof(int);

        return value;
    }

    public int ReadInt()
    {
        return ReadInt32();
    }

    public float ReadSingle()
    {
        return BitConverter.Int32BitsToSingle(ReadInt32());
    }

    public float ReadFloat()
    {
        return ReadSingle();
    }

    public string ReadFixedAsciiString(int length)
    {
        EnsureAvailable(length);

        var valueBytes = _data.AsSpan(Position, length);
        Position += length;

        var terminatorIndex = valueBytes.IndexOf((byte)0);
        if (terminatorIndex >= 0)
        {
            valueBytes = valueBytes[..terminatorIndex];
        }

        return Encoding.ASCII.GetString(valueBytes);
    }

    public string ReadFixedAscii(int length)
    {
        return ReadFixedAsciiString(length);
    }

    private void EnsureAvailable(int byteCount)
    {
        if (byteCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(byteCount), "Byte count cannot be negative.");
        }

        if (Position + byteCount > _data.Length)
        {
            throw new InvalidDataException("Packet does not contain enough bytes for the requested read.");
        }
    }
}
