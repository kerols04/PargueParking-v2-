using System;
using System.Collections.Generic;
using System.IO;

namespace PragueParkingV2.Core
{
    // Den här klassen håller koll på alla priser. Den är statisk så att koden kan nå priserna överallt i programmet
    public static class PriceList
    {
        private static Dictionary<string, decimal> prices = new Dictionary<string, decimal>();
        public static int FreeMinutes { get; private set; } = 10;

        // Här läser programmet in priserna från PriceList.txt-filen.
        public static void LoadPrices(string filePath = "PriceList.txt")
        {
            try
            {
                // Kontrollera om filen finns
                if (!File.Exists(filePath))
                {
                    SetDefaultPrices();
                    return;
                }

                prices.Clear();
                FreeMinutes = 10; // Sätter tillbaka startvärdet

                // Läs alla rader från filen
                string[] lines = File.ReadAllLines(filePath);

                foreach (string line in lines)
                {
                    // Hoppa över tomma rader och kommentarer (börjar med #)
                    if (string.IsNullOrWhiteSpace(line) || line.Trim().StartsWith("#"))
                        continue;

                    // Dela upp rad vid '='
                    string[] parts = line.Split('=');

                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();

                        // Speciell hantering för FreeMinutes
                        if (key == "FreeMinutes")
                        {
                            if (int.TryParse(value, out int minutes))
                            {
                                FreeMinutes = minutes;
                            }
                        }
                        else if (decimal.TryParse(value, out decimal price))
                        {
                            prices[key] = price;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Om något går fel med filen, visa felet och använd standardpriserna.
                Console.WriteLine($"Fel vid laddning av prislista: {ex.Message}");
                SetDefaultPrices();
            }
        }

        // Sätt standardpriser om fil inte finns eller inte går att läsa
        private static void SetDefaultPrices()
        {
            prices["Car"] = 20;
            prices["MC"] = 10;
            FreeMinutes = 10;
        }

        // Hämta pris för en fordonstyp
        public static decimal GetPrice(string vehicleType)
        {
            if (prices.ContainsKey(vehicleType))
            {
                return prices[vehicleType];
            }

            // Standardvärde om typen inte finns
            Console.WriteLine($"Varning: Inget pris hittat för {vehicleType}, använder 0 CZK");
            return 0;
        }
    }
}