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
            // STEG 1: Laddar in alla priser från textfilen
            PriceList.LoadPrices();

            // STEG 2: Laddar in inställningarna för garaget från config.json
            var config = FileManager.LoadConfig();

            // STEG 3: Laddar in sparade fordon från parkingdata.json
            var spots = FileManager.LoadData();

            if (spots != null && spots.Count > 0)
            {
                // Om det fanns sparad data, använd den listan med platser
                PHus.LoadSpots(spots);
            }
            else
            {
                // Om filen var tom, skapar vi nya tomma platser istället
                PHus.ResetSpots(config.TotalSpots);
            }

            // STEG 4: Starta huvudmenyn direkt
            ConsoleUI.Run();

            // STEG 5: När programmet stängs, spara all aktuell data
            Console.WriteLine();
            Console.WriteLine("→ Sparar parkeringsdata...");
            FileManager.SaveData(PHus.GetAllSpots());
            Console.WriteLine();

            // Sista meddelandet innan programmet stängs
            Console.WriteLine("Tack för att du använde systemet!");
        }
    }
}