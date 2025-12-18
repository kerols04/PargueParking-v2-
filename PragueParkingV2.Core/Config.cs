using System;
using System.Collections.Generic;
using System.Linq;

namespace PragueParkingV2.Core
{
    /// <summary>
    /// Inställningar för parkeringen (läses från config.json).
    /// </summary>
    public class Config
    {
        public int TotalSpots { get; set; } = 100;

        // Bakåtkompatibilitet (om du råkar använda SpotSize någonstans)
        public int SpotSize
        {
            get => DefaultSpotSize;
            set => DefaultSpotSize = value;
        }

        public int DefaultSpotSize { get; set; } = 4;

        public List<SpotSizeOverride> SpotSizeOverrides { get; set; } = new();

        public List<VehicleTypeConfig> VehicleTypes { get; set; } = new()
        {
            new VehicleTypeConfig { Key = "Car", DisplayName = "Bil", Size = 4, MaxPerSpot = 1 },
            new VehicleTypeConfig { Key = "MC", DisplayName = "Motorcykel", Size = 1, MaxPerSpot = 4 }
        };

        public int GetSpotSize(int spotNumber)
        {
            if (spotNumber <= 0) return DefaultSpotSize;

            var hit = SpotSizeOverrides
                .FirstOrDefault(o => spotNumber >= o.From && spotNumber <= o.To);

            return (hit is null || hit.Size <= 0) ? DefaultSpotSize : hit.Size;
        }

        public VehicleTypeConfig? GetVehicleType(string typeKey)
        {
            if (string.IsNullOrWhiteSpace(typeKey)) return null;

            return VehicleTypes.FirstOrDefault(v =>
                string.Equals(v.Key, typeKey, StringComparison.OrdinalIgnoreCase));
        }

        public int GetVehicleSize(string typeKey, int fallback)
        {
            var vt = GetVehicleType(typeKey);
            return (vt is null || vt.Size <= 0) ? fallback : vt.Size;
        }

        public int GetMaxPerSpot(string typeKey, int fallback)
        {
            var vt = GetVehicleType(typeKey);
            return (vt is null || vt.MaxPerSpot <= 0) ? fallback : vt.MaxPerSpot;
        }
    }

    public class SpotSizeOverride
    {
        public int From { get; set; } = 1; // inkl.
        public int To { get; set; } = 1;   // inkl.
        public int Size { get; set; } = 4;
    }

    public class VehicleTypeConfig
    {
        public string Key { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public int Size { get; set; } = 1;
        public int MaxPerSpot { get; set; } = 1;
    }
}
