public sealed class PlayerData
{
    public int Id { get; set; }

    public int Money { get; set; }

    public int Bank { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastSeen { get; set; }

    public double DrivenDistance { get; set; }

    public DateTime? LastInterestAt { get; set; }

    public bool IsPoliceAuthorized { get; set; }
}