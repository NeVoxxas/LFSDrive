using System.Text;

namespace LfsCruise.InSim.Packets;

public sealed class BtnPacket : InSimPacket
{
    public const byte PacketType = 45;

    public byte UCID { get; init; }
    public byte ClickID { get; init; }
    public byte Inst { get; init; }
    public byte BStyle { get; init; }
    public byte TypeIn { get; init; }
    public byte L { get; init; }
    public byte T { get; init; }
    public byte W { get; init; }
    public byte H { get; init; }
    public string Text { get; init; } = string.Empty;

    public BtnPacket() : base(0, PacketType, 0) { }

    public byte[] ToArray()
    {
        var textBytes = Encoding.ASCII.GetBytes(Text);
        var textLength = Math.Min(textBytes.Length + 1, 240);
        var paddedTextLength = ((textLength + 3) / 4) * 4;

        var size = 12 + paddedTextLength;
        var packet = new byte[size];

        packet[0] = (byte)(size / 4);
        packet[1] = PacketType;
        packet[2] = 0;      // ReqI
        packet[3] = UCID;

        packet[4] = ClickID;
        packet[5] = Inst;
        packet[6] = BStyle;
        packet[7] = TypeIn;

        packet[8] = L;
        packet[9] = T;
        packet[10] = W;
        packet[11] = H;

        Array.Copy(textBytes, 0, packet, 12, Math.Min(textBytes.Length, paddedTextLength - 1));

        return packet;
    }
}