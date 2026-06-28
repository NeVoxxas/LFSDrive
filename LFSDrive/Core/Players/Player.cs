using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LfsCruise.Core.Players;

public sealed class Player
{
    public byte UCID { get; init; }

    public string Username { get; init; } = string.Empty;

    public string Nickname { get; set; } = string.Empty;

    public bool IsAdmin { get; set; }

    public byte TotalConnections { get; set; }

    public PlayerData Data { get; set; } = new();
}