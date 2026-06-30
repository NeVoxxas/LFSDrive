namespace LfsCruise.Core.Jobs.Taxi

{
    public sealed class TaxiFareConfig
    {
        public int BaseFare { get; set; } = 100;
        public int PricePerKm { get; set; } = 300;
        public int MinReward { get; set; } = 250;
        public int MaxReward { get; set; } = 3000;

    }
}
