using LfsCruise.Core.Jobs;
using LfsCruise.Core.UI;

namespace LfsCruise.Core.UI.Menu.Pages;

public sealed class JobDetailsPage : MenuPage
{
    private readonly JobType _jobType;
    private readonly JobService _jobService;

    public JobDetailsPage(JobType jobType, JobService jobService)
    {
        _jobType = jobType;
        _jobService = jobService;
    }

    public override string Title => GetJobName(_jobType);

    public override IReadOnlyList<MenuButton> GetButtons(MenuContext context)
    {
        var player = context.Player;
        var currentJob = _jobService.GetJob(player);

        var buttons = new List<MenuButton>
        {
            new() { ClickId = ClickIds.Menu.Back, Text = "^7< Atgal" },
            new() { ClickId = ClickIds.Shop.PageInfo, Text = GetDescription(_jobType) }
        };

        if (currentJob == _jobType)
        {
            buttons.Add(new MenuButton
            {
                ClickId = ClickIds.Jobs.Leave,
                Text = "^1Iseiti is darbo"
            });
        }
        else
        {
            buttons.Add(new MenuButton
            {
                ClickId = ClickIds.Jobs.Join,
                Text = currentJob == JobType.None
                    ? "^2Isidarbinti"
                    : "^1Pirma iseik is dabartinio darbo"
            });
        }

        return buttons;
    }

    public override async Task HandleClickAsync(
        MenuManager manager,
        MenuContext context,
        byte clickId,
        CancellationToken cancellationToken)
    {
        if (clickId == ClickIds.Menu.Back)
        {
            await manager.OpenJobsAsync(context.Player, cancellationToken);
            return;
        }

        if (clickId == ClickIds.Jobs.Join)
        {
            await manager.JoinJobFromMenuAsync(context.Player, _jobType, cancellationToken);
            return;
        }

        if (clickId == ClickIds.Jobs.Leave)
        {
            await manager.LeaveJobFromMenuAsync(context.Player, cancellationToken);
            return;
        }
    }

    private static string GetJobName(JobType jobType)
    {
        return jobType switch
        {
            JobType.Taxi => "Taxi",
            JobType.Delivery => "DPD",
            _ => "Darbas"
        };
    }

    private static string GetDescription(JobType jobType)
    {
        return jobType switch
        {
            JobType.Taxi => "^7Vezk keleivius is tasko A i taska B.",
            JobType.Delivery => "^7Pristatyk krovinius i nurodytas vietas.",
            _ => "^7Darbo aprasymas nerastas."
        };
    }
}