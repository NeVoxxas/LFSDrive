using System.Text.RegularExpressions;

namespace LfsCruise.Utils;

public static class LfsText
{
    private static readonly Regex ColorRegex =
        new(@"\^[0-9]", RegexOptions.Compiled);

    public static string StripColors(string text)
    {
        return ColorRegex.Replace(text, "");
    }
}