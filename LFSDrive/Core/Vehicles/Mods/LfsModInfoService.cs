using System.Net.Http;
using System.Text.RegularExpressions;

namespace LfsCruise.Core.Vehicles.Mods;

public sealed class LfsModInfoService
{
    private readonly HttpClient _httpClient = new();
    private static readonly Regex TitleRegex = new(@"<title>(.*?)</title>", RegexOptions.Compiled | RegexOptions.Singleline);

    public async Task<string?> FetchModNameAsync(string skinId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"https://www.lfs.net/files/vehmods/{skinId}";
            var html = await _httpClient.GetStringAsync(url, cancellationToken);

            var match = TitleRegex.Match(html);
            if (!match.Success)
                return null;

            // <title>LFS - Files - Vehicle Mods - ROKU HATCH</title>
            var title = match.Groups[1].Value.Trim();
            const string prefix = "LFS - Files - Vehicle Mods - ";

            return title.StartsWith(prefix) ? title[prefix.Length..].Trim() : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LFS mod info fetch failed for {skinId}: {ex.Message}");
            return null;
        }
    }
}