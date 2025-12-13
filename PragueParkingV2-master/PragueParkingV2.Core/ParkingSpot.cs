using System.Collections.Generic;
using System.Linq;

namespace PragueParkingV2.Core
{
    // Den här klassen håller koll på en enda parkeringsplats.
    public class ParkingSpot
    {
        // Det unika numret som platsen har
        public int SpotNumber { get; set; }

        // Hur stor platsen är totalt (t.ex. 4 enheter)
        public int Capacity { get; set; }

        // En lista över alla fordon som står på just den här rutan
        public List<Vehicle> ParkedVehicles { get; set; }

        // Konstruktor: Körs när en ny ruta skapas
        public ParkingSpot(int spotNumber, int capacity = 4)
        {
            SpotNumber = spotNumber;
            Capacity = capacity;
            ParkedVehicles = new List<Vehicle>();
        }

        // Tom konstruktor: Behövs för att kunna läsa in data från JSON-filen
        public ParkingSpot()
        {
            ParkedVehicles = new List<Vehicle>();
        }

        // Räknar ut hur många enheter plats som är ledig just nu
        public int GetAvailableSpace()
        {
            // Linq Sum: Lägger ihop storleken på alla parkerade fordon
            int usedSpace = ParkedVehicles.Sum(v => v.Size);
            return Capacity - usedSpace; // Total kapacitet minus det som används
        }

        // Kollar snabbt om platsen är helt tom
        public bool IsEmpty()
        {
            return ParkedVehicles.Count == 0;
        }

        // Kollar snabbt om platsen är helt full
        public bool IsFull()
        {
            return GetAvailableSpace() == 0;
        }

        // Försöker parkera ett nytt fordon här
        public bool TryParkVehicle(Vehicle vehicle)
        {
            // Kollar om det finns tillräckligt med plats för fordonets storlek
            if (vehicle.Size <= GetAvailableSpace())
            {
                ParkedVehicles.Add(vehicle);
                return true; // Parkeringen lyckades!
            }
            return false; // Ingen plats
        }

        // Tar bort ett fordon från denna plats och ger tillbaka fordonet
        public Vehicle RemoveVehicle(string regNo)
        {
            // Linq FirstOrDefault: Försöker hitta fordonet genom att jämföra regnumret
            // (OrdinalIgnoreCase gör sökningen skiftlägesokänslig)
            Vehicle vehicle = ParkedVehicles.FirstOrDefault(v => v.RegNo.Equals(regNo, System.StringComparison.OrdinalIgnoreCase));

            if (vehicle != null)
            {
                ParkedVehicles.Remove(vehicle);
                return vehicle; // Hittade fordonet och tog bort det
            }

            return null; // Hittade inte fordonet
        }

        // Ger en enkel textbeskrivning av platsens status (Tom/Full/Delvis)
        public string GetStatusDescription()
        {
            if (IsEmpty())
                return "Tom";
            if (IsFull())
                return "Full";
            // Annars är den delvis fylld
            return $"Delvis ({ParkedVehicles.Count} fordon)";
        }
    }
}