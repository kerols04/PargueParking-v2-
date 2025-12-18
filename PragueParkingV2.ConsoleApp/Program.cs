using PragueParkingV2.Core;
using PragueParkingV2.Data;

namespace PragueParkingV2.ConsoleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Ladda prislista + config först
            PriceList.LoadPrices("PriceList.txt");

            var config = FileManager.LoadConfig();
            PHus.ApplyConfig(config);

            // Ladda parkeringsdata
            var spots = FileManager.LoadData();

            if (spots.Count == 0)
            {
                // Första körningen: skapa tom parkering enligt config
                PHus.ResetFromConfig();
                FileManager.SaveData(PHus.GetAllSpots().ToList());
            }
            else
            {
                // Annars: använd sparad data och justera mot config
                PHus.LoadSpots(spots);
                PHus.EnsureGarageMatchesConfig();
            }

            var ui = new ConsoleUI();
            ui.Run();

            // Spara alltid när programmet avslutas
            FileManager.SaveData(PHus.GetAllSpots().ToList());
        }
    }
}
