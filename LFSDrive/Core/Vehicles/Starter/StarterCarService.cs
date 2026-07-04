namespace LfsCruise.Core.Vehicles.Starter;

public sealed class StarterCarService
{
    private readonly StarterCarConfig _config;

    public StarterCarService(StarterCarStorage storage)
    {
        _config = storage.Load();

        Console.WriteLine($"Starter cars loaded: {_config.Cars.Count}");
    }

    public StarterCarConfig Config => _config;
}