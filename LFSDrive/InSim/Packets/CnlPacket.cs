namespace LfsCruise.InSim.Packets;

public sealed class CnlPacket : InSimPacket
{
    public const byte PacketType = 19;

    private CnlPacket(int size, byte reqI, byte ucid, byte reason, byte total)
        : base(size, PacketType, reqI)
    {
        UCID = ucid;
        Reason = reason;
        Total = total;
    }

    public byte UCID { get; }
    public byte Reason { get; }
    public byte Total { get; }

    public static InSimPacket Parse(byte[] rawData)
    {
        var reader = new PacketReader(rawData);

        var size = reader.ReadByte() * 4;
        var type = reader.ReadByte();
        var reqI = reader.ReadByte();
        var ucid = reader.ReadByte();

        if (type != PacketType)
            throw new InvalidDataException($"Expected IS_CNL {PacketType}, got {type}.");

        var reason = reader.ReadByte();
        var total = reader.ReadByte();
        _ = reader.ReadByte();
        _ = reader.ReadByte();

        return new CnlPacket(size, reqI, ucid, reason, total);
    }
}