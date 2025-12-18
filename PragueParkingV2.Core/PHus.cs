using System;
using System.Collections.Generic;
using System.Linq;

namespace PragueParkingV2.Core
{
    // Representerar hela parkeringshuset (globalt i appen).
    public static class PHus
    {
        private static List<ParkingSpot> parkingSpots = new List<ParkingSpot>();

        // Den config som gäller just nu (laddas i Program.cs).
        public static Config CurrentConfig { get; private set; } = new Config();

        static PHus()
        {
            // Default om config inte hunnit laddas än.
            ResetSpots(100, 4);
        }

        // Sätt config (körs i Program.cs efter LoadConfig).
        public static void ApplyConfig(Config config)
        {
            CurrentConfig = config ?? new Config();

            // Säkerhetsnät ifall någon råkar få null i json.
            CurrentConfig.VehicleTypes ??= new List<string> { "Car", "MC" };
            CurrentConfig.MaxVehiclesPerSpot ??= new Dictionary<string, int> { { "Car", 1 }, { "MC", 2 } };
        }

        public static void LoadSpots(List<ParkingSpot> spots)
        {
            if (spots != null && spots.Count > 0)
            {
                parkingSpots = spots;
                Console.WriteLine($"✓ {spots.Count} parkeringsplatser laddade från fil");
                Console.WriteLine($"✓ {GetAllVehicles().Count} fordon är parkerade");
            }
        }

        // Skapar om garaget med valfritt antal platser + valfri platsstorlek.
        public static void ResetSpots(int numberOfSpots = 100, int spotSize = 4)
        {
            parkingSpots.Clear();

            for (int i = 1; i <= numberOfSpots; i++)
                parkingSpots.Add(new ParkingSpot(i, spotSize));

            Console.WriteLine($"✓ Parkeringshuset initierat med {numberOfSpots} platser (SpotSize={spotSize})");
        }

        // Ser till att garaget matchar config när man laddat från fil.
        public static void EnsureGarageMatchesConfig()
        {
            int desiredCount = CurrentConfig.TotalSpots;
            int desiredSize = CurrentConfig.SpotSize;

            if (parkingSpots == null) parkingSpots = new List<ParkingSpot>();

            // Uppdatera kapacitet på rutor där det är säkert (ingen data går sönder).
            foreach (var spot in parkingSpots)
            {
                int used = spot.Capacity - spot.GetAvailableSpace();
                if (desiredSize >= used)
                    spot.Capacity = desiredSize;
            }

            // Expandera om config vill ha fler platser.
            if (parkingSpots.Count < desiredCount)
            {
                for (int i = parkingSpots.Count + 1; i <= desiredCount; i++)
                    parkingSpots.Add(new ParkingSpot(i, desiredSize));
            }

            // Krymp om config vill ha färre (tar bara bort tomma rutor längst bak).
            while (parkingSpots.Count > desiredCount && parkingSpots.Last().IsEmpty())
            {
                parkingSpots.RemoveAt(parkingSpots.Count - 1);
            }
        }

        public static int GetAvailableSpace() => parkingSpots.Sum(spot => spot.GetAvailableSpace());
        public static List<ParkingSpot> GetAllSpots() => parkingSpots;
        public static List<Vehicle> GetAllVehicles() => parkingSpots.SelectMany(s => s.ParkedVehicles).ToList();

        // Parkerar med hänsyn till config (max/typer) och försöker para ihop MC.
        public static bool ParkVehicle(Vehicle vehicle)
        {
            if (vehicle == null) return false;

            IEnumerable<ParkingSpot> sortedSpots = parkingSpots.OrderBy(s => s.SpotNumber);

            // Extra smart: om det är en MC, prioritera rutor som redan har exakt 1 MC.
            if (vehicle is MC)
            {
                sortedSpots = parkingSpots
                    .OrderByDescending(s =>
                        s.ParkedVehicles.Count == 1 &&
                        s.ParkedVehicles[0] is MC &&
                        !s.IsFull(CurrentConfig))
                    .ThenBy(s => s.SpotNumber);
            }

            foreach (var spot in sortedSpots)
            {
                if (spot.TryParkVehicle(vehicle, CurrentConfig))
                    return true;
            }

            return false;
        }

        public static Vehicle RetrieveVehicle(string regNo)
        {
            foreach (var spot in parkingSpots)
            {
                Vehicle vehicle = spot.RemoveVehicle(regNo);
                if (vehicle != null) return vehicle;
            }
            return null;
        }

        // Flytt: säkert (tappar inte bort fordonet om parkering misslyckas).
        public static bool MoveVehicle(string regNo)
        {
            var fromSpot = parkingSpots.FirstOrDefault(s =>
                s.ParkedVehicles.Any(v => v.RegNo.Equals(regNo, StringComparison.OrdinalIgnoreCase)));

            if (fromSpot == null) return false;

            var vehicle = fromSpot.RemoveVehicle(regNo);
            if (vehicle == null) return false;

            if (ParkVehicle(vehicle))
                return true;

            // Om vi inte hittar ny plats: lägg tillbaka på originalrutan.
            fromSpot.TryParkVehicle(vehicle, CurrentConfig);
            return false;
        }

        public static Vehicle FindVehicle(string regNo) =>
            GetAllVehicles().FirstOrDefault(v => v.RegNo.Equals(regNo, StringComparison.OrdinalIgnoreCase));

        public static int FindSpotNumber(string regNo)
        {
            foreach (var spot in parkingSpots)
            {
                if (spot.ParkedVehicles.Any(v => v.RegNo.Equals(regNo, StringComparison.OrdinalIgnoreCase)))
                    return spot.SpotNumber;
            }
            return -1;
        }

        public static int CountEmptySpots() => parkingSpots.Count(s => s.IsEmpty());
        public static int CountFullSpots() => parkingSpots.Count(s => s.IsFull(CurrentConfig));
        public static int CountPartialSpots() => parkingSpots.Count(s => !s.IsEmpty() && !s.IsFull(CurrentConfig));
    }
}
