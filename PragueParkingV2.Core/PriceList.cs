using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace PragueParkingV2.Core
{
    public static class PriceList
    {
        private static Dictionary<string, decimal> prices = new Dictionary<string, decimal>();
        public static int FreeMinutes { get; private set; } = 10;

        public static void LoadPrices(string filePath = "PriceList.txt")
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    SetDefaultPrices();
                    return;
                }

                prices.Clear();
                FreeMinutes = 10;

                foreach (var rawLine in File.ReadAllLines(filePath))
                {
                    // 1) Ta bort inline-kommentarer: "Car=20 # bil" -> "Car=20"
                    string line = rawLine.Split('#')[0].Trim();

                    // 2) Hoppa över tomma rader
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    // 3) Key=Value
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

                    // Decimal: först invariant (.), annars current culture (, i Sverige)
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
    }
}
