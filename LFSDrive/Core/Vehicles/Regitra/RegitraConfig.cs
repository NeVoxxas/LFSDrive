namespace LfsCruise.Core.Vehicles.Regitra;

public sealed class RegitraConfig
{
    public int PlatePrice { get; set; } = 500;
    public int PlateChangePrice { get; set; } = 1000;
    public int InsurancePrice { get; set; } = 300;
    public int InspectionPrice { get; set; } = 200;

    public int InsuranceDurationDays { get; set; } = 14;
    public int InspectionDurationMonths { get; set; } = 1;
}