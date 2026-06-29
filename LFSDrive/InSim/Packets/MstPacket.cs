using System.Text;

namespace LfsCruise.InSim.Packets;

public sealed class MstPacket : InSimPacket
{
    public const byte PacketType = 13; // ISP_MST

    public string Msg { get; init; } = string.Empty;

    public MstPacket() : base(68, PacketType, 0)
    {
    }

    public byte[] ToArray()
    {
        var packet = new byte[68];

        packet[0] = 17; // 68 / 4
        packet[1] = PacketType;
        packet[2] = 0; // ReqI must be 0
        packet[3] = 0;

        var textBytes = Encoding.ASCII.GetBytes(Msg);
        var length = Math.Min(textBytes.Length, 63);

        Array.Copy(textBytes, 0, packet, 4, length);
        packet[67] = 0;

        return packet;
    }
}