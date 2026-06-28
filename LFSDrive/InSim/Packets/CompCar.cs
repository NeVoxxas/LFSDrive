namespace LfsCruise.InSim.Packets;

public sealed class CompCar
{
    public ushort Node { get; init; }
    public ushort Lap { get; init; }
    public byte PLID { get; init; }
    public byte Position { get; init; }
    public byte Info { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public int Z { get; init; }
    public ushort Speed { get; init; }
    public ushort Direction { get; init; }
    public ushort Heading { get; init; }
    public short AngVel { get; init; }

    public double XMetres => X / 65536.0;
    public double YMetres => Y / 65536.0;
    public double ZMetres => Z / 65536.0;
    public double SpeedMs => Speed * 100.0 / 32768.0;
    public double SpeedKmh => SpeedMs * 3.6;
}