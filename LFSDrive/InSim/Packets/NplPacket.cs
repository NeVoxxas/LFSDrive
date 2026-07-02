namespace LfsCruise.InSim.Packets;

public sealed class NplPacket : InSimPacket
{
    public const byte PacketType = 21;

    private NplPacket(int size, byte reqI, byte plid, byte ucid, string playerName, string plate,
        string carName, string carCode, bool isMod, string skinId)
        : base(size, PacketType, reqI)
    {
        PLID = plid;
        UCID = ucid;
        PlayerName = playerName;
        Plate = plate;
        CarName = carName;
        CarCode = carCode;
        IsMod = isMod;
        SkinId = skinId;
    }

    public byte PLID { get; }
    public byte UCID { get; }
    public string PlayerName { get; }
    public string Plate { get; }
    public string CarName { get; }

    public string CarCode { get; }

    public bool IsMod { get; }

    public string SkinId { get; }

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
        _ = reader.ReadByte();   // PType
        _ = reader.ReadUInt16(); // Flags

        var playerName = reader.ReadFixedAsciiString(24);
        var plate = reader.ReadFixedAsciiString(8);

        var carOffset = reader.Position;
        var carBytes = rawData.Skip(carOffset).Take(4).ToArray();

        var carName = reader.ReadFixedAsciiString(4);
        var carCode = BitConverter.ToString(rawData, carOffset, 4);

        var isMod = !(IsAlphaNumeric(carBytes[0]) && IsAlphaNumeric(carBytes[1]) && IsAlphaNumeric(carBytes[2]));

        var skinIdValue = (carBytes[2] << 16) | (carBytes[1] << 8) | carBytes[0];
        var skinId = skinIdValue.ToString("X6");

        _ = reader.ReadFixedAsciiString(16); // SName
        _ = reader.ReadByte(); // Tyre FL
        _ = reader.ReadByte(); // Tyre FR
        _ = reader.ReadByte(); // Tyre RL
        _ = reader.ReadByte(); // Tyre RR

        _ = reader.ReadByte(); // H_Mass
        _ = reader.ReadByte(); // H_TRes
        _ = reader.ReadByte(); // Model
        _ = reader.ReadByte(); // Pass

        _ = reader.ReadByte(); // RWAdj
        _ = reader.ReadByte(); // FWAdj
        _ = reader.ReadByte(); // RIFlags
        _ = reader.ReadByte(); // Sp3

        _ = reader.ReadByte(); // SetF
        _ = reader.ReadByte(); // NumP
        _ = reader.ReadByte(); // Config
        _ = reader.ReadByte(); // Fuel

        return new NplPacket(size, reqI, plid, ucid, playerName, plate, carName, carCode, isMod, skinId);
    }

    private static bool IsAlphaNumeric(byte b)
    {
        return (b >= (byte)'0' && b <= (byte)'9')
            || (b >= (byte)'A' && b <= (byte)'Z')
            || (b >= (byte)'a' && b <= (byte)'z');
    }
}