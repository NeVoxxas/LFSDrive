namespace LfsCruise.Core.Jobs.Police;

public sealed class PoliceJob : IJob
{
    private readonly PoliceConfig _config;
    private readonly PursuitService _pursuitService;

    public PoliceJob(PoliceConfigStorage storage, PursuitService pursuitService)
    {
        _config = storage.Load();
        _pursuitService = pursuitService;
    }

    public JobType Type => JobType.Police;
    public string Name => "Policija";

    public async Task OnStartAsync(JobContext context, CancellationToken cancellationToken)
    {
        var player = context.Player;

        // Policijos pareigunu tapti reikia leidimo (zr. !setpolice admin komanda).
        // JobManager JAU priskyre sita darba PRIES kviesdamas OnStartAsync, tad
        // jei zaidejas neturi leidimo, PRIVALOME rankiniu budu ji atsaukti -
        // kitaip jis liktu "uzstriges" su Police darbu, bet be jokio funkcionalumo.
        if (!player.Data.IsPoliceAuthorized)
        {
            context.JobService.LeaveJob(player);

            await context.SendMessage(
                player.UCID,
                "^1Tu neturi leidimo buti policijos pareigunu. Kreipkis i administracija.",
                cancellationToken);

            return;
        }

        await context.SendMessage(
            player.UCID,
            "^2Policijos pamaina pradeta. ^7Naudok policijos automobili.",
            cancellationToken);
    }

    public async Task OnStopAsync(JobContext context, CancellationToken cancellationToken)
    {
        _pursuitService.RemoveOfficer(context.Player.UCID);

        await context.SendMessage(context.Player.UCID, "^1Policijos pamaina baigta.", cancellationToken);
    }

    public Task OnPlayerMoveAsync(JobContext context, CancellationToken cancellationToken)
    {
        // Sioje "foundation" versijoje pareigūno paties judėjimas nieko
        // specialaus nedaro - visa "gaudymo" logika (atstumo/lygio tikrinimas)
        // yra PursuitService, kuris kviečiamas VISIEMS žaidėjams (ne tik
        // policijai) is MciHandler, nes reikia sekti TIKSLINIO zaidejo, ne tik
        // pareigūno, pozicijas.
        return Task.CompletedTask;
    }
}