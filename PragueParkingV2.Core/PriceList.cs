using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace PragueParkingV2.Core
{
    public static class PriceList
    {
        // Case-insensitive så “car” och “Car” funkar likadant
        private static readonly Dictionary<string, decimal> prices =
            new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        public static int FreeMinutes { get; private set; } = 10;

        public static void LoadPrices(string filePath = "PriceList.txt")
        {
            try
            {
                // Letar både i current dir och i output-mappen
                string resolved = File.Exists(filePath)
                    ? filePath
                    : Path.Combine(AppContext.BaseDirectory, filePath);

                if (!File.Exists(resolved))
                {
                    SetDefaultPrices();
                    return;
                }

                prices.Clear();
                FreeMinutes = 10;

                foreach (var rawLine in File.ReadAllLines(resolved))
                {
                    // Tar bort inline-kommentarer: "Car=20 # bil" -> "Car=20"
                    string line = rawLine.Split('#')[0].Trim();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    string[] parts = line.Split('=');
                    if (parts.Length != 2) continue;

                    string key = parts[0].Trim();
                    string value = parts[1].Trim();

                    if (key.Equals("FreeMinutes", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(value, out int minutes))
                            FreeMinutes = minutes;
                        continue;
                    }

                    if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price) ||
                        decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out price))
                    {
                        prices[key] = price;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid laddning av prislista: {ex.Message}");
                SetDefaultPrices();
            }
        }

        private static void SetDefaultPrices()
        {
            // Standardpriser (CZK/h)
            prices["Car"] = 20;
            prices["MC"] = 10;
            FreeMinutes = 10;
        }

        public static decimal GetPrice(string vehicleType)
        {
            if (prices.TryGetValue(vehicleType, out decimal price))
                return price;

            Console.WriteLine($"Varning: Inget pris hittat för {vehicleType}, använder 0 CZK");
            return 0;
        }

        // Behövs för UI som listar alla priser
        public static IReadOnlyDictionary<string, decimal> GetAllPrices()
            => new Dictionary<string, decimal>(prices, StringComparer.OrdinalIgnoreCase);
    }
}
