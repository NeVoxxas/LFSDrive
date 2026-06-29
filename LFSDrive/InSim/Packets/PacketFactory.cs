namespace LfsCruise.InSim.Packets;

public sealed class PacketFactory
{
    private readonly IReadOnlyDictionary<byte, Func<byte[], InSimPacket>> _parsers;

    public PacketFactory()
        : this(new Dictionary<byte, Func<byte[], InSimPacket>>
        {
            [VerPacket.PacketType] = VerPacket.Parse,
            [TinyPacket.PacketType] = TinyPacket.Parse,
            [NcnPacket.PacketType] = NcnPacket.Parse,
            [CnlPacket.PacketType] = CnlPacket.Parse,
            [MsoPacket.PacketType] = MsoPacket.Parse,
            [MciPacket.PacketType] = MciPacket.Parse,
            [NplPacket.PacketType] = NplPacket.Parse,
            [PitPacket.PacketType] = PitPacket.Parse,
            [PsfPacket.PacketType] = PsfPacket.Parse
        })
    {
    }

    public PacketFactory(IReadOnlyDictionary<byte, Func<byte[], InSimPacket>> parsers)
    {
        _parsers = parsers;
    }

    public InSimPacket Create(byte[] rawData)
    {
        if (rawData.Length < 3)
        {
            throw new InvalidDataException("InSim packet header must contain Size, Type and ReqI.");
        }

        var type = rawData[1];

        return _parsers.TryGetValue(type, out var parser)
            ? parser(rawData)
            : new UnknownPacket(rawData);
    }
}
