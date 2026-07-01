using LfsCruise.Core.Economy.Bank;

namespace LfsCruise.Core.UI.Menu.Pages;

public sealed class BankMenuPage : MenuPage
{
    private const byte WithdrawMaxChars = 8;
    private const byte DepositMaxChars = 8;

    private readonly BankService _bankService;
    private readonly bool _allowTransactions; // true = žaidėjas fiziškai banke (atvažiavo į zoną)
    private readonly bool _showBackButton;

    public BankMenuPage(BankService bankService, bool allowTransactions, bool showBackButton)
    {
        _bankService = bankService;
        _allowTransactions = allowTransactions;
        _showBackButton = showBackButton;
    }

    public override string Title => "Bankas";

    public override IReadOnlyList<MenuButton> GetButtons(MenuContext context)
    {
        var player = context.Player;
        var buttons = new List<MenuButton>();

        if (_showBackButton)
        {
            buttons.Add(new MenuButton { ClickId = ClickIds.Menu.Back, Text = "^7< Atgal" });
        }

        buttons.Add(new MenuButton
        {
            ClickId = ClickIds.Bank.Balance,
            Text = $"^7Balansas: ^2${player.Data.Bank}",
            Enabled = false
        });

        buttons.Add(new MenuButton
        {
            ClickId = ClickIds.Bank.InterestInfo,
            Text = FormatInterestStatus(_bankService.GetInterestStatus(player)),
            Enabled = false
        });

        if (_allowTransactions)
        {
            buttons.Add(new MenuButton
            {
                ClickId = ClickIds.Bank.Deposit,
                Text = "^2Ideti pinigu",
                TypeIn = DepositMaxChars
            });

            buttons.Add(new MenuButton
            {
                ClickId = ClickIds.Bank.Withdraw,
                Text = "^3Isgryninti",
                TypeIn = WithdrawMaxChars
            });
        }
        else
        {
            buttons.Add(new MenuButton
            {
                ClickId = ClickIds.Bank.Withdraw,
                Text = "^7Isgryninti (tik banke)",
                Enabled = false
            });
        }

        buttons.Add(new MenuButton
        {
            ClickId = ClickIds.Bank.History,
            Text = "^7Pinigu istorija"
        });

        return buttons;
    }

    public override Task HandleClickAsync(
        MenuManager manager,
        MenuContext context,
        byte clickId,
        CancellationToken cancellationToken)
    {
        if (clickId == ClickIds.Menu.Back && _showBackButton)
            return manager.OpenMainMenuAsync(context.Player, cancellationToken);

        if (clickId == ClickIds.Bank.History)
            return manager.OpenBankHistoryAsync(context.Player, _allowTransactions, _showBackButton, 0, cancellationToken);

        return Task.CompletedTask;
    }

    public override async Task HandleTypeInAsync(
        MenuManager manager,
        MenuContext context,
        byte clickId,
        string text,
        CancellationToken cancellationToken)
    {
        var player = context.Player;

        // Apsauga: net jei kažkas nusiųstų IS_BTT be aktyvaus mygtuko UI-je,
        // serveris vis tiek tikrina _allowTransactions.
        if (!_allowTransactions)
        {
            await manager.SendMessageAsync(player.UCID, "^1Sia operacija galima atlikti tik banke.", cancellationToken);
            return;
        }

        if (clickId == ClickIds.Bank.Deposit)
        {
            if (!int.TryParse(text, out var amount) || amount <= 0)
            {
                await manager.SendMessageAsync(player.UCID, "^1Neteisinga suma.", cancellationToken);
                return;
            }

            if (!await _bankService.DepositAsync(player, amount, cancellationToken))
            {
                await manager.SendMessageAsync(player.UCID, "^1Neuztenka pinigu.", cancellationToken);
                return;
            }

            await manager.SendMessageAsync(player.UCID, $"^2Ideta: ^7${amount}", cancellationToken);
            await manager.OpenBankAsync(player, _allowTransactions, _showBackButton, cancellationToken);
            return;
        }

        if (clickId == ClickIds.Bank.Withdraw)
        {
            if (!int.TryParse(text, out var amount) || amount <= 0)
            {
                await manager.SendMessageAsync(player.UCID, "^1Neteisinga suma.", cancellationToken);
                return;
            }

            if (!await _bankService.WithdrawAsync(player, amount, cancellationToken))
            {
                await manager.SendMessageAsync(player.UCID, "^1Neuztenka pinigu banke.", cancellationToken);
                return;
            }

            await manager.SendMessageAsync(player.UCID, $"^2Isgryninta: ^7${amount}", cancellationToken);
            await manager.OpenBankAsync(player, _allowTransactions, _showBackButton, cancellationToken);
        }
    }

    private static string FormatInterestStatus(BankInterestStatus status)
    {
        if (status.IsPaused)
            return "^1Palukanos: sustabdytos";

        var remaining = status.Remaining;

        return $"^7Palukanos uz: ^3{(int)remaining.TotalMinutes:D2}:{remaining.Seconds:D2}";
    }
}