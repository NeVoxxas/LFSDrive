using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LfsCruise.InSim.Packets;

public sealed class TinyPacket : InSimPacket
{
    public const byte PacketType = 3;

    public byte SubT { get; }

    private TinyPacket(int size, byte reqI, byte subT)
        : base(size, PacketType, reqI)
    {
        SubT = subT;
    }

    public static InSimPacket Parse(byte[] rawData)
    {
        var reader = new PacketReader(rawData);

        var size = reader.ReadByte() * 4;
        var type = reader.ReadByte();
        var reqI = reader.ReadByte();
        var subT = reader.ReadByte();

        return new TinyPacket(size, reqI, subT);
    }

    public byte[] ToArray()
    {
        return new byte[]
        {
            1,          // Size = 4 bytes / 4
            PacketType, // Type = IS_TINY
            ReqI,
            SubT
        };
    }
}