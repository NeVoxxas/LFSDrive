using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LfsCruise.Core.Vehicles.Tow;

public sealed class TowConfig
{

    public double GarageX { get; set; }
    public double GarageY{ get; set; }

    public int BaseFare { get; set; } = 0;
    public int PricePerKm { get; set; } = 100;
    public int MinPrice { get; set; } = 0;
    public int MaxPrice { get; set; } = 500;
}
