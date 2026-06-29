namespace LfsCruise.InSim.Packets;

public sealed class BtcPacket : InSimPacket
{
    public const byte PacketType = 46; // ISP_BTC

    private BtcPacket(
        int size,
        byte reqI,
        byte ucid,
        byte clickId,
        byte inst,
        byte cFlags)
        : base(size, PacketType, reqI)
    {
        UCID = ucid;
        ClickID = clickId;
        Inst = inst;
        CFlags = cFlags;
    }

    public byte UCID { get; }
    public byte ClickID { get; }
    public byte Inst { get; }
    public byte CFlags { get; }

    public static InSimPacket Parse(byte[] rawData)
    {
        var reader = new PacketReader(rawData);

        var size = reader.ReadByte() * 4;
        var type = reader.ReadByte();
        var reqI = reader.ReadByte();
        var ucid = reader.ReadByte();

        var clickId = reader.ReadByte();
        var inst = reader.ReadByte();
        var cFlags = reader.ReadByte();

        _ = reader.ReadByte();

        return new BtcPacket(size, reqI, ucid, clickId, inst, cFlags);
    }
}