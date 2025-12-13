using System;

namespace PragueParkingV2.Core
{
    // Motorcykel klassen ärver från Vehicle, så den får alla grundfunktioner.
    public class MC : Vehicle
    {
        // Konstruktor: Körs när en ny MC skapas (anropar basklassens konstruktor först)
        public MC(string regNo) : base(regNo)
        {
            Size = 1; // En MC tar bara 1 enhet plats
        }
        // Tom konstruktor: Behövs för att kunna läsa in sparad data från JSON-filen
        public MC() : base()
        {
            Size = 1;
        }
        // Här implementerar MC klassen den obligatoriska funktionen för timpris
        public override decimal GetHourlyRate()
        {
            // Hämtar timpriset för MC från prislistan
            return PriceList.GetPrice("MC");
        }
        // Returnerar ett läsbart namn för fordonstypen
        public override string GetVehicleTypeName()
        {
            return "Motorcykel";
        }
    }
}