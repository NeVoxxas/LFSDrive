namespace LfsCruise.Core.Vehicles.Mods;

public sealed class ModNameService
{
    private readonly ModNameStorage _storage;
    private readonly ModNameConfig _config;
    private readonly LfsModInfoService _modInfoService;

    public ModNameService(ModNameStorage storage, LfsModInfoService modInfoService)
    {
        _storage = storage;
        _config = storage.Load();
        _modInfoService = modInfoService;
    }

    public string? GetCachedName(string skinId)
    {
        return _config.Names.TryGetValue(skinId, out var name) ? name : null;
    }

    public async Task<string> ResolveNameAsync(string skinId, CancellationToken cancellationToken = default)
    {
        var cached = GetCachedName(skinId);
        if (cached is not null)
            return cached;

        var fetched = await _modInfoService.FetchModNameAsync(skinId, cancellationToken);
        var resolved = fetched ?? $"Modas ({skinId})";

        SetName(skinId, resolved);
        return resolved;
    }

    public void SetName(string skinId, string name)
    {
        _config.Names[skinId] = name;
        _storage.Save(_config);
    }
}