using LfsCruise.Core.Players;
using LfsCruise.Core.Vehicles.Regitra;

namespace LfsCruise.Core.Commands.Vehicles;

public sealed class RegitraCommand : ICommand
{
    private readonly RegitraService _regitraService;
    private readonly Func<byte, string, CancellationToken, Task> _sendMessage;

    public RegitraCommand(RegitraService regitraService, Func<byte, string, CancellationToken, Task> sendMessage)
    {
        _regitraService = regitraService;
        _sendMessage = sendMessage;
    }

    public string Name => "regitra";
    public string Description => "Rodo automobilio dokumentu busena.";
    public bool AdminOnly => false;

    public async Task ExecuteAsync(Player player, string[] args, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(player.CarName))
        {
            await _sendMessage(player.UCID, "^1Tu nesi automobilyje.", cancellationToken);
            return;
        }

        var docs = await _regitraService.GetDocumentsAsync(player, player.CarName, cancellationToken);

        var plate = docs?.HasPlate == true ? docs.PlateNumber : "nera";
        var insurance = docs?.HasValidInsurance == true ? docs.InsuranceExpiresAt!.Value.ToString("yyyy-MM-dd") : "negalioja";
        var inspection = docs?.HasValidInspection == true ? docs.InspectionExpiresAt!.Value.ToString("yyyy-MM-dd") : "negalioja";
        var overall = docs?.IsValid == true ? "^2Galioja" : "^1Negalioja";

        await _sendMessage(
            player.UCID,
            $"^7Numeriai: ^2{plate} ^7| Draudimas iki: ^2{insurance} ^7| T.A iki: ^2{inspection} ^7| Busena: {overall}",
            cancellationToken);
    }
}