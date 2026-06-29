namespace LfsCruise.Core.UI.Menu;

public abstract class MenuPage
{
    public abstract string Title { get; }

    public abstract IReadOnlyList<MenuButton> GetButtons(MenuContext context);

    public virtual Task HandleClickAsync(
        MenuManager manager,
        MenuContext context,
        byte clickId,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}