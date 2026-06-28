namespace LfsCruise.Core.Events;

public sealed class EventBus
{
    private readonly Dictionary<Type, List<object>> _handlers = new();

    public void Subscribe<T>(IEventHandler<T> handler)
    {
        var type = typeof(T);

        if (!_handlers.TryGetValue(type, out var list))
        {
            list = new List<object>();
            _handlers[type] = list;
        }

        list.Add(handler);
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
    {
        if (!_handlers.TryGetValue(typeof(T), out var list))
            return;

        foreach (var handler in list.Cast<IEventHandler<T>>())
        {
            await handler.HandleAsync(@event, cancellationToken);
        }
    }
}