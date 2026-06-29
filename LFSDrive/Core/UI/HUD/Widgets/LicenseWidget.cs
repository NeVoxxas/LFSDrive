using LfsCruise.Core.Players;
using LfsCruise.Core.Progression;
using LfsCruise.Core.UI.HUD;

namespace LfsCruise.Core.UI.HUD.Widgets;

public sealed class LicenseWidget : HudWidget
{
    private readonly ProgressionService _progressionService;

    public LicenseWidget(ProgressionService progressionService)
    {
        _progressionService = progressionService;
    }

    public override byte ClickId => 50;
    public override byte Width => HudLayout.LicenseWidth;

    public override string GetText(Player player)
    {
        var license = _progressionService.GetLicense(player);
        return $"^7Lic ^2{license:F1}";
    }
}