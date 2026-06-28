namespace LfsCruise.InSim.Packets;

[AttributeUsage(AttributeTargets.Class)]
public sealed class InSimPacketTypeAttribute : Attribute
{
    public InSimPacketTypeAttribute(byte type)
    {
        Type = type;
    }

    public byte Type { get; }
}
