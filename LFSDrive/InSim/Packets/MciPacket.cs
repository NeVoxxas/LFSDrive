using System.Xml.Linq;

namespace LfsCruise.InSim.Packets;

public sealed class MciPacket : InSimPacket
{
    public const byte PacketType = 38;

    private MciPacket(int size, byte reqI, byte numC, IReadOnlyList<CompCar> cars)
        : base(size, PacketType, reqI)
    {
        NumC = numC;
        Cars = cars;
    }

    public byte NumC { get; }
    public IReadOnlyList<CompCar> Cars { get; }

    public static InSimPacket Parse(byte[] rawData)
    {
        var reader = new PacketReader(rawData);

        var size = reader.ReadByte() * 4;
        var type = reader.ReadByte();
        var reqI = reader.ReadByte();
        var numC = reader.ReadByte();

        if (type != PacketType)
            throw new InvalidDataException($"Expected IS_MCI {PacketType}, got {type}.");

        var cars = new List<CompCar>();

        for (var i = 0; i < numC; i++)
        {
            var node = reader.ReadUInt16();
            var lap = reader.ReadUInt16();
            var plid = reader.ReadByte();
            var position = reader.ReadByte();
            var info = reader.ReadByte();
            _ = reader.ReadByte(); // Sp3

            cars.Add(new CompCar
            {
                Node = node,
                Lap = lap,
                PLID = plid,
                Position = position,
                Info = info,
                X = reader.ReadInt32(),
                Y = reader.ReadInt32(),
                Z = reader.ReadInt32(),
                Speed = reader.ReadUInt16(),
                Direction = reader.ReadUInt16(),
                Heading = reader.ReadUInt16(),
                AngVel = reader.ReadInt16()
            });
        }

        return new MciPacket(size, reqI, numC, cars);
    }
}