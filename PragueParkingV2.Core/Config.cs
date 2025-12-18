using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace PragueParkingV2.Core
{
    // Läser in och håller alla inställningar från config.json
    public class Config
    {
        public int TotalSpots { get; set; } = 100;

        // Standardstorlek för en plats (t.ex. 4 = exakt en bil)
        public int DefaultSpotSize { get; set; } = 4;

        // Bakåtkomp: om någon råkar ha "SpotSize" i sin json så funkar det ändå
        [JsonPropertyName("SpotSize")]
        public int SpotSizeAlias
        {
            get => DefaultSpotSize;
            set => DefaultSpotSize = value;
        }

        // Låter vissa intervall ha annan platsstorlek (kravet “olika storlek”)
        public List<SpotSizeOverride> SpotSizeOverrides { get; set; } = new();

        // Fordonstyper som stöds + deras storlek + max antal per plats
        public List<VehicleTypeDefinition> VehicleTypes { get; set; } = new()
        {
            new VehicleTypeDefinition { Key = "Car", DisplayName = "Bil",        Size = 4, MaxPerSpot = 1 },
            new VehicleTypeDefinition { Key = "MC",  DisplayName = "Motorcykel", Size = 2, MaxPerSpot = 2 }
        };

        // Returnerar korrekt platsstorlek för ett visst platsnummer
        public int GetSpotSize(int spotNumber)
        {
            foreach (var ov in SpotSizeOverrides)
            {
                if (spotNumber >= ov.From && spotNumber <= ov.To)
                    return ov.Size;
            }
            return DefaultSpotSize;
        }

        public VehicleTypeDefinition? GetVehicleType(string key)
            => VehicleTypes.FirstOrDefault(v => v.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

        // Säkerställer att config aldrig har “trasiga” värden
        public void Normalize()
        {
            if (TotalSpots < 1) TotalSpots = 1;
            if (DefaultSpotSize < 1) DefaultSpotSize = 1;

            VehicleTypes ??= new();
            SpotSizeOverrides ??= new();

            if (VehicleTypes.Count == 0)
            {
                VehicleTypes.Add(new VehicleTypeDefinition { Key = "Car", DisplayName = "Bil", Size = 4, MaxPerSpot = 1 });
                VehicleTypes.Add(new VehicleTypeDefinition { Key = "MC",  DisplayName = "Motorcykel", Size = 2, MaxPerSpot = 2 });
            }

            foreach (var v in VehicleTypes)
            {
                v.Key = (v.Key ?? "").Trim();
                v.DisplayName = (v.DisplayName ?? "").Trim();
                if (v.Size < 1) v.Size = 1;
                if (v.MaxPerSpot < 1) v.MaxPerSpot = 1;
            }

            foreach (var ov in SpotSizeOverrides)
            {
                if (ov.From < 1) ov.From = 1;
                if (ov.To < ov.From) ov.To = ov.From;
                if (ov.Size < 1) ov.Size = 1;
            }
        }
    }

    // “Plats 1–10 har storlek 8” osv.
    public class SpotSizeOverride
    {
        public int From { get; set; }
        public int To { get; set; }
        public int Size { get; set; }
    }

    // Definition av en fordonstyp i config.json
    public class VehicleTypeDefinition
    {
        public string Key { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public int Size { get; set; } = 4;
        public int MaxPerSpot { get; set; } = 1;
    }
}
