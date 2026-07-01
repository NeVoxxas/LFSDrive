using LfsCruise.Core.Economy.Bank;

namespace LfsCruise.Core.UI.Menu.Pages;

public sealed class BankHistoryPage : MenuPage
{
    private const int PageSize = 5;

    private readonly bool _allowTransactions;
    private readonly bool _showBackButton;
    private readonly int _page;
    private readonly IReadOnlyList<BankTransaction> _items;
    private readonly int _totalCount;

    public BankHistoryPage(
        bool allowTransactions,
        bool showBackButton,
        int page,
        IReadOnlyList<BankTransaction> items,
        int totalCount)
    {
        _allowTransactions = allowTransactions;
        _showBackButton = showBackButton;
        _page = Math.Max(0, page);
        _items = items;
        _totalCount = totalCount;
    }

    public override string Title => "Pinigu istorija";

    public override IReadOnlyList<MenuButton> GetButtons(MenuContext context)
    {
        var totalPages = Math.Max(1, (int)Math.Ceiling(_totalCount / (double)PageSize));
        var currentPage = Math.Clamp(_page, 0, totalPages - 1);

        var buttons = new List<MenuButton>
        {
            new() { ClickId = ClickIds.Menu.Back, Text = "^7< Atgal" }
        };

        byte clickId = ClickIds.Bank.HistoryEntryStart;

        foreach (var item in _items)
        {
            buttons.Add(new MenuButton
            {
                ClickId = clickId++,
                Text = FormatEntry(item),
                Enabled = false
            });
        }

        if (_items.Count == 0)
        {
            buttons.Add(new MenuButton { ClickId = clickId, Text = "^7Tranzakciju nera", Enabled = false });
        }

        if (currentPage > 0)
        {
            buttons.Add(new MenuButton { ClickId = ClickIds.Bank.HistoryPrev, Text = "^7< Ankstesnis" });
        }

        buttons.Add(new MenuButton
        {
            ClickId = ClickIds.Bank.HistoryPageInfo,
            Text = $"^7Puslapis {currentPage + 1}/{totalPages}",
            Enabled = false
        });

        if (currentPage < totalPages - 1)
        {
            buttons.Add(new MenuButton { ClickId = ClickIds.Bank.HistoryNext, Text = "^7Kitas >" });
        }

        return buttons;
    }

    public override Task HandleClickAsync(
        MenuManager manager,
        MenuContext context,
        byte clickId,
        CancellationToken cancellationToken)
    {
        if (clickId == ClickIds.Menu.Back)
            return manager.OpenBankAsync(context.Player, _allowTransactions, _showBackButton, cancellationToken);

        if (clickId == ClickIds.Bank.HistoryPrev)
            return manager.OpenBankHistoryAsync(context.Player, _allowTransactions, _showBackButton, _page - 1, cancellationToken);

        if (clickId == ClickIds.Bank.HistoryNext)
            return manager.OpenBankHistoryAsync(context.Player, _allowTransactions, _showBackButton, _page + 1, cancellationToken);

        return Task.CompletedTask;
    }

    private static string FormatEntry(BankTransaction transaction)
    {
        var (sign, color) = transaction.Type == BankTransactionType.Withdraw
            ? ("-", "^1")
            : ("+", "^2");

        var label = transaction.Type switch
        {
            BankTransactionType.Deposit => "Idejimas",
            BankTransactionType.Withdraw => "Isgryninimas",
            BankTransactionType.Interest => "Palukanos",
            _ => "?"
        };

        return $"^7{transaction.CreatedAt:MM-dd HH:mm} {label} {color}{sign}{transaction.Amount}";
    }
}