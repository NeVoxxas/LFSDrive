namespace LfsCruise.InSim.Packets;

public sealed class UnknownPacket : InSimPacket
{
    public UnknownPacket(byte[] rawData)
        : base(GetPacketSize(rawData), GetPacketType(rawData), GetRequestId(rawData))
    {
        RawData = rawData.ToArray();
    }

    public byte[] RawData { get; }

    private static int GetPacketSize(byte[] rawData)
    {
        EnsureHeader(rawData);

        return rawData[0] * 4;
    }

    private static byte GetPacketType(byte[] rawData)
    {
        EnsureHeader(rawData);

        return rawData[1];
    }

    private static byte GetRequestId(byte[] rawData)
    {
        EnsureHeader(rawData);

        return rawData[2];
    }

    private static void EnsureHeader(byte[] rawData)
    {
        if (rawData.Length < 3)
        {
            throw new InvalidDataException("InSim packet header must contain Size, Type and ReqI.");
        }
    }
}
