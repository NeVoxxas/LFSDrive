namespace LfsCruise.InSim.Packets;

public sealed class JrrPacket : InSimPacket
{
    public const byte PacketType = 58; // ISP_JRR

    public const byte ActionReject = 0;         // JRR_REJECT
    public const byte ActionSpawn = 1;          // JRR_SPAWN
    public const byte ActionReset = 4;          // JRR_RESET

    // PLID = 0   -> atsakymas i join request (naudoja UCID)
    // PLID != 0  -> perkelti ESAMA automobili (UCID ignoruojamas)
    public byte PLID { get; init; }
    public byte UCID { get; init; }
    public byte JRRAction { get; init; } = ActionSpawn;

    public JrrPacket() : base(16, PacketType, 0)
    {
    }

    public byte[] ToArray()
    {
        return
        [
            4,          // Size / 4 = 16 / 4
            PacketType, // ISP_JRR
            0,          // ReqI
            PLID,       // PATAISYTA: anksciau visada buvo 0

            UCID,
            JRRAction,
            0, // Sp2
            0, // Sp3

            // StartPos (ObjectInfo, 8 baitu) - visi nuliai = LFS default start point
            0, 0, 0, 0, 0, 0, 0, 0
        ];
    }
}