namespace LfsCruise.InSim.Packets;

public sealed class NplPacket : InSimPacket
{
    public const byte PacketType = 21;

    private NplPacket(int size, byte reqI, byte plid, byte ucid, string playerName, string plate, string carName)
        : base(size, PacketType, reqI)
    {
        PLID = plid;
        UCID = ucid;
        PlayerName = playerName;
        Plate = plate;
        CarName = carName;
    }

    public byte PLID { get; }
    public byte UCID { get; }
    public string PlayerName { get; }
    public string Plate { get; }
    public string CarName { get; }

    public static InSimPacket Parse(byte[] rawData)
    {
        var reader = new PacketReader(rawData);

        var size = reader.ReadByte() * 4;
        var type = reader.ReadByte();
        var reqI = reader.ReadByte();
        var plid = reader.ReadByte();

        if (type != PacketType)
            throw new InvalidDataException($"Expected IS_NPL {PacketType}, got {type}.");

        var ucid = reader.ReadByte();
        _ = reader.ReadByte(); // PType
        _ = reader.ReadUInt16(); // Flags

        var playerName = reader.ReadFixedAsciiString(24);
        var plate = reader.ReadFixedAsciiString(8);
        var carName = reader.ReadFixedAsciiString(4);

        return new NplPacket(size, reqI, plid, ucid, playerName, plate, carName);
    }
}