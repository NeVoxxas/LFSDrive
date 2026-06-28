namespace LfsCruise.InSim.Packets;

public sealed class MsoPacket : InSimPacket
{
    public const byte PacketType = 11;

    private MsoPacket(int size, byte reqI, byte ucid, byte plid, byte userType, byte textStart, string message)
        : base(size, PacketType, reqI)
    {
        UCID = ucid;
        PLID = plid;
        UserType = userType;
        TextStart = textStart;
        Message = message;
        Text = textStart < message.Length ? message[textStart..] : message;
    }

    public byte UCID { get; }
    public byte PLID { get; }
    public byte UserType { get; }
    public byte TextStart { get; }
    public string Message { get; }
    public string Text { get; }

    public static InSimPacket Parse(byte[] rawData)
    {
        var reader = new PacketReader(rawData);

        var size = reader.ReadByte() * 4;
        var type = reader.ReadByte();
        var reqI = reader.ReadByte();
        _ = reader.ReadByte();

        if (type != PacketType)
            throw new InvalidDataException($"Expected IS_MSO {PacketType}, got {type}.");

        var ucid = reader.ReadByte();
        var plid = reader.ReadByte();
        var userType = reader.ReadByte();
        var textStart = reader.ReadByte();

        var msgLength = size - 8;
        var message = reader.ReadFixedAsciiString(msgLength);

        return new MsoPacket(size, reqI, ucid, plid, userType, textStart, message);
    }
}