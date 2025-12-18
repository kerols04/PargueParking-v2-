using System.Collections.Generic;

namespace PragueParkingV2.Core
{
    // Håller alla inställningar som ska kunna ändras utan att ändra kod.
    public class Config
    {
        // Hur många platser garaget ska ha (default: 100).
        public int TotalSpots { get; set; } = 100;

        // Hur stor en plats är i "enheter" (default: 4).
        public int SpotSize { get; set; } = 4;

        // Vilka fordonstyper som är tillåtna (nycklarna matchar klassnamn: "Car", "MC").
        public List<string> VehicleTypes { get; set; } = new List<string> { "Car", "MC" };

        // Max antal fordon per plats, per typ.
        // Ex: Car=1, MC=2 ger klassiska regeln "1 bil eller 2 MC per ruta".
        public Dictionary<string, int> MaxVehiclesPerSpot { get; set; } =
            new Dictionary<string, int> { { "Car", 1 }, { "MC", 2 } };
    }
}

