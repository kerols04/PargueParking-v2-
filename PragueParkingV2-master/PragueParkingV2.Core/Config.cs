using System.Collections.Generic;

namespace PragueParkingV2.Core
{
    // Den här klassen håller koll på de viktigaste inställningarna för hela garaget.
    public class Config
    {
        // Totalt antal parkeringsplatser i garaget
        public int TotalSpots { get; set; }

        // Hur stor varje enskild parkeringsplats är (i enheter, t.ex. 4)
        public int SpotSize { get; set; }

        // Konstruktor: Sätter startvärden när programmet körs
        public Config()
        {
            TotalSpots = 100; // Standardvärdet är 100 platser
            SpotSize = 4; // Standardstorleken för en plats är 4 enheter
        }
    }

}