using System.Text;

namespace LfsCruise.InSim.Packets;

public sealed class MtcPacket : InSimPacket
{
    public const byte PacketType = 14;

    public byte UCID { get; init; }
    public byte PLID { get; init; }
    public byte Sound { get; init; } = 1;
    public string Text { get; init; } = string.Empty;

    public MtcPacket()
        : base(0, PacketType, 0)
    {
    }

    public byte[] ToArray()
    {
        var textBytes = Encoding.ASCII.GetBytes(Text);

        var textLength = Math.Min(textBytes.Length + 1, 128);
        var paddedTextLength = ((textLength + 3) / 4) * 4;

        var size = 8 + paddedTextLength;
        var packet = new byte[size];

        packet[0] = (byte)(size / 4);
        packet[1] = PacketType;
        packet[2] = 0;
        packet[3] = Sound;

        packet[4] = UCID;
        packet[5] = PLID;
        packet[6] = 0;
        packet[7] = 0;

        Array.Copy(textBytes, 0, packet, 8, Math.Min(textBytes.Length, paddedTextLength - 1));

        return packet;
    }
}