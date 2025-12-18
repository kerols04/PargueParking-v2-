namespace PragueParkingV2.Core
{
    /// <summary>
    /// Globalt "minne" av config under körning.
    /// </summary>
    public static class GarageSettings
    {
        public static Config Current { get; private set; } = new Config();

        public static void Apply(Config? config)
        {
            Current = config ?? new Config();
        }
    }
}
