using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Concurrent;

namespace LfsCruise.Core.Players;

public sealed class PlayerManager
{
    private readonly ConcurrentDictionary<byte, Player> _players = new();

    public IEnumerable<Player> Players => _players.Values;

    public bool Add(Player player)
    {
        return _players.TryAdd(player.UCID, player);
    }

    public bool Remove(byte ucid)
    {
        return _players.TryRemove(ucid, out _);
    }

    public Player? Get(byte ucid)
    {
        _players.TryGetValue(ucid, out var player);
        return player;
    }

    public bool Exists(byte ucid)
    {
        return _players.ContainsKey(ucid);
    }

    public int Count => _players.Count;
}
