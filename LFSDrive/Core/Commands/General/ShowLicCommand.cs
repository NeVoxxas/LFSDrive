using LfsCruise.Core.Economy;
using LfsCruise.Core.Players;
using LfsCruise.Core.Progression;
using System.Globalization;
using System.Numerics;

namespace LfsCruise.Core.Commands.Economy;

public sealed class ShowLicCommand : ICommand
{
    /*public enum CommandPermission
    {
        Player,
        Admin,
        Moderator,
        Developer
    }*/

    public bool AdminOnly => false;

    private readonly ProgressionService _progressionService;
    private readonly Func<byte, string, CancellationToken, Task> _sendMessage;

    public ShowLicCommand(ProgressionService progressionService, Func<byte, string, CancellationToken, Task> sendMessage)
    {
        _progressionService = progressionService;
        _sendMessage = sendMessage;
    }

    public string Name => "lic";
    public string Description => "Licenzijos";

    public async Task ExecuteAsync(Player player, string[] args, CancellationToken cancellationToken)
    {
        var license = _progressionService.GetLicense(player);

        await _sendMessage(
            player.UCID,
            $"LIC: {license:F1} | KM: {player.Data.DrivenDistance:F1}",
            cancellationToken
        );


    }
}