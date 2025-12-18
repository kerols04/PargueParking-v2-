using PragueParkingV2.ConsoleApp;
using PragueParkingV2.Core;
using PragueParkingV2.Data;
using System;

namespace PragueParkingV2.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1) Prislista först (behövs för avgifter + tester).
            PriceList.LoadPrices();

            // 2) Ladda config och aktivera den i PHus.
            var config = FileManager.LoadConfig();
            PHus.ApplyConfig(config);

            // 3) Ladda sparad data (om den finns).
            var spots = FileManager.LoadData();

            if (spots != null && spots.Count > 0)
            {
                PHus.LoadSpots(spots);

                // Säkerställer att garage-storlek/SpotSize följer config när det går.
                PHus.EnsureGarageMatchesConfig();
            }
            else
            {
                // Om ingen data finns: skapa nytt garage enligt config.
                PHus.ResetSpots(config.TotalSpots, config.SpotSize);
            }

            // 4) Starta menyn.
            ConsoleUI.Run();

            // 5) Spara vid avslut.
            Console.WriteLine();
            Console.WriteLine("→ Sparar parkeringsdata...");
            FileManager.SaveData(PHus.GetAllSpots());
            Console.WriteLine("Tack för att du använde systemet!");
        }
    }
}
