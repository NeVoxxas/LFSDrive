namespace LfsCruise.InSim.Packets;

[InSimPacketType(PacketType)]
public sealed class VerPacket : InSimPacket
{
    public const byte PacketType = 2;

    private VerPacket(
        int size,
        byte reqI,
        string version,
        string product,
        byte inSimVersion)
        : base(size, PacketType, reqI)
    {
        Version = version;
        Product = product;
        InSimVersion = inSimVersion;
    }

    public string Version { get; }

    public string Product { get; }

    public byte InSimVersion { get; }

    public static InSimPacket Parse(byte[] rawData)
    {
        var reader = new PacketReader(rawData);

        var size = reader.ReadByte() * 4;
        var type = reader.ReadByte();
        var reqI = reader.ReadByte();
        _ = reader.ReadByte();

        if (type != PacketType)
        {
            throw new InvalidDataException($"Expected IS_VER packet type {PacketType}, but received {type}.");
        }

        var version = reader.ReadFixedAsciiString(8);
        var product = reader.ReadFixedAsciiString(6);
        var inSimVersion = reader.ReadByte();
        _ = reader.ReadByte();

        return new VerPacket(size, reqI, version, product, inSimVersion);
    }
}
