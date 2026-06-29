namespace LfsCruise.InSim.Packets;

public sealed class PsfPacket : InSimPacket
{
    public const byte PacketType = 27;

    private PsfPacket(int size, byte reqI, byte plid, uint stopTime)
        : base(size, PacketType, reqI)
    {
        PLID = plid;
        StopTime = stopTime;
    }

    public byte PLID { get; }
    public uint StopTime { get; }

    public static InSimPacket Parse(byte[] rawData)
    {
        var reader = new PacketReader(rawData);

        var size = reader.ReadByte() * 4;
        var type = reader.ReadByte();
        var reqI = reader.ReadByte();
        var plid = reader.ReadByte();

        if (type != PacketType)
            throw new InvalidDataException($"Expected IS_PSF {PacketType}, got {type}.");

        var stopTime = reader.ReadUInt32();

        return new PsfPacket(size, reqI, plid, stopTime);
    }
}