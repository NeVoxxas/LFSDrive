public sealed class VehicleState
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }

    public double Speed { get; set; }

    public double Heading { get; set; }

    public ushort Node { get; set; }

    public byte PLID { get; set; }
}