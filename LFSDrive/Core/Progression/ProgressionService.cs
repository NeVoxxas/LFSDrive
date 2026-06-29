namespace LfsCruise.Core.Progression;

using LfsCruise.Core.Players;

public sealed class ProgressionService
{
    public void AddDistance(Player player, double distanceKm)
    {
        if (distanceKm <= 0)
            return;

        player.Data.DrivenDistance += distanceKm;
    }

    public double GetLicense(Player player)
    {
        return Math.Floor((player.Data.DrivenDistance / 15.0) * 10.0) / 10.0;
    }
}