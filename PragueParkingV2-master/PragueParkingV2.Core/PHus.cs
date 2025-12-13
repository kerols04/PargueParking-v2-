using System;
using System.Collections.Generic;
using System.Linq;

namespace PragueParkingV2.Core
{
    // Den här klassen representerar hela parkeringshuset. 
    // Den är statisk så att alla delar av programmet kan nå den enkelt.
    public static class PHus
    {
        // En lista som sparar alla parkeringsplatser
        private static List<ParkingSpot> parkingSpots = new List<ParkingSpot>();

        // Körs automatiskt när programmet startar för första gången
        static PHus()
        {
            // Skapa 100 tomma platser som standard
            ResetSpots(100);
        }

        // Ladda in sparade parkeringsplatser från fil
        public static void LoadSpots(List<ParkingSpot> spots)
        {
            if (spots != null && spots.Count > 0)
            {
                parkingSpots = spots;
                Console.WriteLine($"✓ {spots.Count} parkeringsplatser laddade från fil");

                // Räkna antal parkerade fordon
                int totalVehicles = GetAllVehicles().Count;
                Console.WriteLine($"✓ {totalVehicles} fordon är parkerade");
            }
        }

        // Gör garaget tomt, används vid start eller omstart 
        public static void ResetSpots(int numberOfSpots = 100)
        {
            parkingSpots.Clear();

            // Här bör SpotSize hämtas från Config.cs, men för enkelhet behåller vi standardvärdet 4
            for (int i = 1; i <= numberOfSpots; i++)
            {
                // Skapar en ny plats med nummer (i) och standardstorlek (4)
                parkingSpots.Add(new ParkingSpot(i, 4));
            }

            Console.WriteLine($"✓ Parkeringshuset initierat med {numberOfSpots} platser");
        }

        // Räknar ut totalt hur mycket plats som är ledig i hela garaget
        public static int GetAvailableSpace()
        {
            // Linq Sum: Summerar ledig plats från alla rutor
            return parkingSpots.Sum(spot => spot.GetAvailableSpace());
        }

        // Hämtar listan över alla rutor i garaget
        public static List<ParkingSpot> GetAllSpots()
        {
            return parkingSpots;
        }

        // Hämtar en lista med alla fordon som är parkerade just nu
        public static List<Vehicle> GetAllVehicles()
        {
            // Linq SelectMany: Samlar ihop alla fordon från alla rutors listor till en enda lista
            return parkingSpots.SelectMany(spot => spot.ParkedVehicles).ToList();
        }

        // Försöker parkera ett fordon på första bästa lediga plats
        public static bool ParkVehicle(Vehicle vehicle)
        {
            // Sorterar platserna för att optimera (får MC att parkera ihop)
            var sortedSpots = parkingSpots
                // Första prioritet: Hitta platser som redan har en MC och som har plats över
                .OrderByDescending(s => s.ParkedVehicles.Any(v => v is MC) && s.GetAvailableSpace() >= vehicle.Size)
                // Andra prioritet: Välj nästa lediga plats
                .ThenBy(s => s.SpotNumber);

            // Går igenom platserna i den ordning vi sorterade dem
            foreach (var spot in sortedSpots)
            {
                // Försöker parkera fordonet på den här platsen
                if (spot.TryParkVehicle(vehicle))
                {
                    return true; // Lyckades parkera!
                }
            }

            return false; // Hittade ingen plats någonstans
        }

        // Tar bort och hämtar ut ett fordon från parkeringen
        public static Vehicle RetrieveVehicle(string regNo)
        {
            // Går igenom alla rutor och försöker ta bort fordonet
            foreach (var spot in parkingSpots)
            {
                Vehicle vehicle = spot.RemoveVehicle(regNo);

                if (vehicle != null)
                {
                    return vehicle; // Hittade och tog bort fordonet!
                }
            }

            return null; // Hittade inget fordon med det regnumret
        }

        // Flyttar ett fordon till en ny ledig plats
        public static bool MoveVehicle(string regNo)
        {
            // 1. Först tar vi ut fordonet från dess gamla plats
            Vehicle vehicle = RetrieveVehicle(regNo);

            if (vehicle == null)
            {
                return false; // Hittade inget fordon att flytta
            }

            // 2. Sedan parkerar vi det igen (ParkVehicle hittar automatiskt en ny plats)
            return ParkVehicle(vehicle);
        }

        // Letar upp ett specifikt fordon utan att ta bort det
        public static Vehicle FindVehicle(string regNo)
        {
            // Linq: Hittar det första fordonet som matchar regnumret, skiftlägesokänsligt
            return GetAllVehicles().FirstOrDefault(v => v.RegNo.Equals(regNo, StringComparison.OrdinalIgnoreCase));
        }

        // Hittar vilket nummer en plats har, baserat på fordonets regnummer
        public static int? FindSpotNumber(string regNo)
        {
            // Linq: Hittar den första rutan som innehåller fordonet
            var spot = parkingSpots.FirstOrDefault(s => s.ParkedVehicles.Any(v => v.RegNo.Equals(regNo, StringComparison.OrdinalIgnoreCase)));

            // Om rutan finns, ge SpotNumber, annars null
            return spot?.SpotNumber;
        }

        // Räknar alla rutor som är helt tomma
        public static int CountEmptySpots()
        {
            // Linq Count: Räknar alla rutor där IsEmpty() är sann
            return parkingSpots.Count(spot => spot.IsEmpty());
        }

        // Räknar alla rutor som är helt fulla
        public static int CountFullSpots()
        {
            // Linq Count: Räknar alla rutor där IsFull() är sann
            return parkingSpots.Count(spot => spot.IsFull());
        }

        // Räknar alla rutor som har fordon, men inte är fulla (halvfulla)
        public static int CountPartialSpots()
        {
            // Linq Count: Räknar rutor som inte är tomma OCH inte är fulla
            return parkingSpots.Count(spot => !spot.IsEmpty() && !spot.IsFull());
        }
    }
}