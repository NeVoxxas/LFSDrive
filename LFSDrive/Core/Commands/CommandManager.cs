using LfsCruise.Core.Players;

namespace LfsCruise.Core.Commands;

public sealed class CommandManager
{
    private readonly Dictionary<string, ICommand> _commands = new();

    public void Register(ICommand command)
    {
        _commands[command.Name.ToLower()] = command;
    }

    public async Task ExecuteAsync(Player player, string text, CancellationToken cancellationToken)
    {
        if (!text.StartsWith('!'))
            return;

        var parts = text[1..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return;

        var commandName = parts[0].ToLower();
        var args = parts.Skip(1).ToArray();

        if (_commands.TryGetValue(commandName, out var command))
        {
            await command.ExecuteAsync(player, args, cancellationToken);
            return;
        }
    }
}