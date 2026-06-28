namespace LfsCruise.InSim.Packets;

public abstract class InSimPacket
{
    protected InSimPacket(int size, byte type, byte reqI)
    {
        Size = size;
        Type = type;
        ReqI = reqI;
    }

    public int Size { get; init; }

    public byte Type { get; init; }

    public byte ReqI { get; init; }
}
