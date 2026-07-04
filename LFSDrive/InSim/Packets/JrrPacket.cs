namespace LfsCruise.InSim.Packets;

public sealed class JrrPacket : InSimPacket
{
    public const byte PacketType = 58; // ISP_JRR

    public const byte ActionReject = 0; // JRR_REJECT
    public const byte ActionSpawn = 1;  // JRR_SPAWN

    public byte PLID { get; init; }      // 0 kai atsakoma į join request
    public byte UCID { get; init; }
    public byte JRRAction { get; init; } = ActionSpawn;

    public JrrPacket() : base(16, PacketType, 0)
    {
    }

    public byte[] ToArray()
    {
        var packet = new byte[16];

        packet[0] = 4;          // Size / 4 = 16 / 4
        packet[1] = PacketType; // ISP_JRR
        packet[2] = 0;          // ReqI
        packet[3] = 0;          // PLID - ZERO, nes tai atsakymas į join request

        packet[4] = UCID;
        packet[5] = JRRAction;
        packet[6] = 0; // Sp2
        packet[7] = 0; // Sp3

        // StartPos (ObjectInfo, 8 baitai) - visi nuliai = naudoti default start point

        return packet;
    }
}