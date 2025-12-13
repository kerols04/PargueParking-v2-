using System;

namespace PragueParkingV2.Core
{
    // Bil-klassen ärver från Vehicle, så den får alla grundfunktioner.
    public class Car : Vehicle
    {
        // Konstruktor: Körs när en ny Bil skapas (anropar basklassens konstruktor först)
        public Car(string regNo) : base(regNo)
        {
            Size = 4; // En bil tar 4 enheter plats
        }
         // Tom konstruktor: Behövs för att kunna läsa in sparad data från JSON-filen
        public Car() : base()
        {
            Size = 4;
        }
        // Här implementerar Bil-klassen den obligatoriska funktionen för timpris
        public override decimal GetHourlyRate()
        {
            // Hämtar timpriset för Bil från prislistan
            return PriceList.GetPrice("Car");
        }
        // Ger det svenska namnet "Bil" för att visa i menyn
        public override string GetVehicleTypeName()
        {
            return "Bil";
        }
    }
}