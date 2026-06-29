using System.Buffers.Binary;

namespace LfsCruise.InSim.Packets;

public sealed class PitPacket : InSimPacket
{
    public const byte PacketType = 26;

    private PitPacket(
        int size,
        byte reqI,
        byte plid,
        byte fuelAdd,
        byte penalty,
        byte numStops,
        uint work)
        : base(size, PacketType, reqI)
    {
        PLID = plid;
        FuelAdd = fuelAdd;
        Penalty = penalty;
        NumStops = numStops;
        Work = work;
    }

    public byte PLID { get; }
    public byte FuelAdd { get; }
    public byte Penalty { get; }
    public byte NumStops { get; }
    public uint Work { get; }

    public static InSimPacket Parse(byte[] rawData)
    {
        var reader = new PacketReader(rawData);

        var size = reader.ReadByte() * 4;
        var type = reader.ReadByte();
        var reqI = reader.ReadByte();
        var plid = reader.ReadByte();

        if (type != PacketType)
            throw new InvalidDataException($"Expected IS_PIT {PacketType}, got {type}.");

        _ = reader.ReadUInt16(); // LapsDone
        _ = reader.ReadUInt16(); // Flags

        var fuelAdd = reader.ReadByte();
        var penalty = reader.ReadByte();
        var numStops = reader.ReadByte();
        _ = reader.ReadByte(); // Sp3

        _ = reader.ReadByte(); // Tyre FL
        _ = reader.ReadByte(); // Tyre FR
        _ = reader.ReadByte(); // Tyre RL
        _ = reader.ReadByte(); // Tyre RR

        var work = reader.ReadUInt32();

        _ = reader.ReadUInt32(); // Spare

        return new PitPacket(size, reqI, plid, fuelAdd, penalty, numStops, work);
    }
}