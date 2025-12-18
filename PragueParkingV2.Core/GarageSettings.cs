namespace PragueParkingV2.Core
{
    // Enkel “global” åtkomst till aktuell config (så Car/MC kan läsa storlek)
    public static class GarageSettings
    {
        public static Config Current { get; private set; } = new Config();

        public static void Apply(Config? config)
        {
            Current = config ?? new Config();
            Current.Normalize();
        }

        public static int GetVehicleSize(string key, int fallback = 1)
            => Current.GetVehicleType(key)?.Size ?? fallback;

        public static int GetMaxPerSpot(string key, int fallback = 1)
            => Current.GetVehicleType(key)?.MaxPerSpot ?? fallback;

        public static string GetDisplayName(string key, string fallback)
            => Current.GetVehicleType(key)?.DisplayName ?? fallback;
    }
}
