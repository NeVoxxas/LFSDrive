namespace LfsCruise.Core.Vehicles.Regitra;

public sealed class VehicleDocuments
{
    public string? PlateNumber { get; set; }
    public DateTime? InsuranceExpiresAt { get; set; }
    public DateTime? InspectionExpiresAt { get; set; }

    public bool HasPlate => !string.IsNullOrWhiteSpace(PlateNumber);

    public bool HasValidInsurance =>
        InsuranceExpiresAt is { } expiresAt && expiresAt > DateTime.UtcNow;

    public bool HasValidInspection =>
        InspectionExpiresAt is { } expiresAt && expiresAt > DateTime.UtcNow;

    // Jeigu yra visi 3 == galiojantis dok.
    public bool IsValid => HasPlate && HasValidInsurance && HasValidInspection;
}