using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LfsCruise.Core.Jobs.Police;

public sealed class PoliceViolationStorage
{

    private const string FilePath = "Data/police_violations.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    { 
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public PoliceViolationConfig Load()
    {
        if (!File.Exists(FilePath))
        {
            Console.WriteLine($"Police violations config not found, using defaults: {Path.GetFullPath(FilePath)}");
            return new PoliceViolationConfig();
        }

        var json = File.ReadAllText(FilePath);

        return JsonSerializer.Deserialize<PoliceViolationConfig>(json, JsonOptions) ?? new PoliceViolationConfig();
    }

    public void Save(PoliceViolationConfig config)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);

        var json = JsonSerializer.Serialize(config, JsonOptions);

        File.WriteAllText(FilePath, json);
    }
}
