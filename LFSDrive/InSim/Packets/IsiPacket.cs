using System.Buffers.Binary;
using System.Text;

namespace LfsCruise.InSim.Packets;

public sealed class IsiPacket : InSimPacket
{
    public const byte PacketSize = 44;
    public const byte PacketType = 1;

    public IsiPacket()
        : base(PacketSize, PacketType, 1)
    {
    }

    public ushort UDPPort { get; init; }
    public ushort Flags { get; init; }
    public byte InSimVer { get; init; } = 10;
    public byte Prefix { get; init; }
    public ushort Interval { get; init; }
    public string Admin { get; init; } = string.Empty;
    public string IName { get; init; } = string.Empty;

    public byte[] ToArray()
    {
        var packet = new byte[Size];

        packet[0] = (byte)(Size / 4);
        packet[1] = Type;
        packet[2] = ReqI;
        packet[3] = 0;

        BinaryPrimitives.WriteUInt16LittleEndian(packet.AsSpan(4, 2), UDPPort);
        BinaryPrimitives.WriteUInt16LittleEndian(packet.AsSpan(6, 2), Flags);

        packet[8] = InSimVer;
        packet[9] = Prefix;

        BinaryPrimitives.WriteUInt16LittleEndian(packet.AsSpan(10, 2), Interval);

        WriteFixedAscii(Admin, packet.AsSpan(12, 16));
        WriteFixedAscii(IName, packet.AsSpan(28, 16));

        return packet;
    }

    private static void WriteFixedAscii(string value, Span<byte> destination)
    {
        destination.Clear();

        var sourceLength = Math.Min(value.Length, destination.Length);
        Encoding.ASCII.GetBytes(value.AsSpan(0, sourceLength), destination);
    }
}