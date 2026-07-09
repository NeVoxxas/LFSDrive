namespace LfsCruise.Core.Jobs.Police;

public sealed class PoliceViolationConfig
{
    public double OfficerSharePercent { get; set; } = 0.2; // Kiek % baudos atitenka mentui

    public List<PoliceViolation> Violations { get; set; } = new()
    {
        new() { Id = "speeding", Name = "Greicio virsijimas", Fine = 200 },
        new() { Id = "nostop", Name = "Nesustojo prie STOP", Fine = 150 },
        new() { Id = "nodocs", Name = "Vairavimas be dokumentu", Fine = 300 },
        new() { Id = "reckless", Name = "Pavojingas vairavimas", Fine = 250 },
        new() { Id = "wrongway", Name = "Vaziavimas pries eismo kryptimi", Fine = 350 }
    };
}