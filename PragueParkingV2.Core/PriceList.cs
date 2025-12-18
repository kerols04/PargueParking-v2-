using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace PragueParkingV2.Core
{
    public static class PriceList
    {
        private static readonly Dictionary<string, decimal> prices = new();
        public static int FreeMinutes { get; private set; }

        public static string DefaultFilePath => Path.Combine(AppContext.BaseDirectory, "PriceList.txt");

        public static void LoadPrices(string filePath = null)
        {
            filePath = string.IsNullOrWhiteSpace(filePath) ? DefaultFilePath : filePath;

            if (!Path.IsPathRooted(filePath))
                filePath = Path.Combine(AppContext.BaseDirectory, filePath);

            try
            {
                if (!File.Exists(filePath))
                {
                    SetDefaultPrices();
                    SaveDefaultPriceFile(filePath);
                    return;
                }

                prices.Clear();
                FreeMinutes = 0;

                foreach (var rawLine in File.ReadAllLines(filePath))
                {
                    var line = rawLine;
                    int hashIndex = line.IndexOf('#');
                    if (hashIndex >= 0)
                        line = line.Substring(0, hashIndex);

                    line = line.Trim();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var parts = line.Split('=', 2);
                    if (parts.Length != 2)
                        continue;

                    string key = parts[0].Trim();
                    string valueString = parts[1].Trim();

                    if (key.Equals("FreeMinutes", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(valueString, NumberStyles.Integer, CultureInfo.InvariantCulture, out int minutes))
                            FreeMinutes = minutes;
                        continue;
                    }

                    if (decimal.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value))
                        prices[key] = value;
                }

                if (!prices.ContainsKey("Car") || !prices.ContainsKey("MC"))
                    SetDefaultPrices();
            }
            catch
            {
                SetDefaultPrices();
            }
        }

        public static decimal GetPrice(string vehicleType)
            => prices.TryGetValue(vehicleType, out var price) ? price : 0;

        public static IReadOnlyDictionary<string, decimal> GetAllPrices() => prices;

        private static void SetDefaultPrices()
        {
            prices.Clear();
            prices["Car"] = 20;
            prices["MC"] = 10;
            FreeMinutes = 10;
        }

        private static void SaveDefaultPriceFile(string filePath)
        {
            var content =
@"# Prague Parking V2 - Prislista
# Format: KEY=VALUE
# Kommentarer kan stå efter # (inline).

Car=20           # kr per timme
MC=10            # kr per timme
FreeMinutes=10   # gratis minuter innan debitering
";
            File.WriteAllText(filePath, content);
        }
    }
}
