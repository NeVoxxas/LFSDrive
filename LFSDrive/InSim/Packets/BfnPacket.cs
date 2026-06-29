namespace LfsCruise.InSim.Packets;

public sealed class BfnPacket : InSimPacket
{
    public const byte PacketType = 42; // ISP_BFN

    public byte SubT { get; init; }
    public byte UCID { get; init; }
    public byte ClickID { get; init; }
    public byte ClickMax { get; init; }
    public byte Inst { get; init; }

    public BfnPacket() : base(2, PacketType, 0)
    {
    }

    public byte[] ToArray()
    {
        return
        [
            2,              // Size (8 bytes / 4)
            PacketType,     // ISP_BFN
            1,              // ReqI (non-zero)
            SubT,

            UCID,
            ClickID,
            ClickMax,
            Inst
        ];
    }
}