namespace LfsCruise.InSim.Packets;

public sealed class NcnPacket : InSimPacket
{
    public const byte PacketType = 18;

    private NcnPacket(
        int size,
        byte reqI,
        byte ucid,
        string username,
        string playerName,
        bool isAdmin,
        byte total,
        byte flags)
        : base(size, PacketType, reqI)
    {
        UCID = ucid;
        Username = username;
        PlayerName = playerName;
        IsAdmin = isAdmin;
        Total = total;
        Flags = flags;
    }

    public byte UCID { get; }
    public string Username { get; }
    public string PlayerName { get; }
    public bool IsAdmin { get; }
    public byte Total { get; }
    public byte Flags { get; }

    public static InSimPacket Parse(byte[] rawData)
    {
        var reader = new PacketReader(rawData);

        var size = reader.ReadByte() * 4;
        var type = reader.ReadByte();
        var reqI = reader.ReadByte();
        var ucid = reader.ReadByte();

        if (type != PacketType)
        {
            throw new InvalidDataException($"Expected IS_NCN packet type {PacketType}, but received {type}.");
        }

        var username = reader.ReadFixedAsciiString(24);
        var playerName = reader.ReadFixedAsciiString(24);

        var admin = reader.ReadByte();
        var total = reader.ReadByte();
        var flags = reader.ReadByte();
        _ = reader.ReadByte();

        return new NcnPacket(
            size,
            reqI,
            ucid,
            username,
            playerName,
            admin == 1,
            total,
            flags);
    }
}