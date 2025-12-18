using System;
using PragueParkingV2.Core;
using PragueParkingV2.Data;

namespace PragueParkingV2.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            PriceList.LoadPrices();

            var config = FileManager.LoadConfig();
            GarageSettings.Apply(config);

            var loadedSpots = FileManager.LoadData();

            if (loadedSpots != null && loadedSpots.Count > 0)
            {
                PHus.LoadSpots(loadedSpots);
                EnsureSpotCount(config);
            }
            else
            {
                PHus.ResetSpots(config.TotalSpots);
            }

            ConsoleUI.Run();

            FileManager.SaveData(PHus.GetAllSpots());
            Console.WriteLine("Tack för att du använde systemet!");
        }

        private static void EnsureSpotCount(Config config)
        {
            var spots = PHus.GetAllSpots();
            if (spots.Count >= config.TotalSpots) return;

            for (int i = spots.Count + 1; i <= config.TotalSpots; i++)
            {
                spots.Add(new ParkingSpot(i, config.GetSpotSize(i)));
            }
        }
    }
}
