namespace LfsCruise.InSim.Packets;

public sealed class BttPacket : InSimPacket
{
    public const byte PacketType = 47; // ISP_BTT

    private BttPacket(int size, byte reqI, byte ucid, byte clickId, byte inst, byte typeIn, string text)
        : base(size, PacketType, reqI)
    {
        UCID = ucid;
        ClickID = clickId;
        Inst = inst;
        TypeIn = typeIn;
        Text = text;
    }

    public byte UCID { get; }
    public byte ClickID { get; }
    public byte Inst { get; }
    public byte TypeIn { get; }
    public string Text { get; }

    public static InSimPacket Parse(byte[] rawData)
    {
        var reader = new PacketReader(rawData);

        var size = reader.ReadByte() * 4;
        var type = reader.ReadByte();
        var reqI = reader.ReadByte();
        var ucid = reader.ReadByte();

        if (type != PacketType)
            throw new InvalidDataException($"Expected IS_BTT {PacketType}, got {type}.");

        var clickId = reader.ReadByte();
        var inst = reader.ReadByte();
        var typeIn = reader.ReadByte();
        _ = reader.ReadByte(); // Sp3

        var text = reader.ReadFixedAsciiString(96);

        return new BttPacket(size, reqI, ucid, clickId, inst, typeIn, text);
    }
}