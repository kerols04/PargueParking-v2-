using System;
using System.Collections.Generic;
using System.Linq;

namespace PragueParkingV2.Core
{
    public static class PHus
    {
        private static List<ParkingSpot> _spots = new List<ParkingSpot>();
        public static Config CurrentConfig { get; private set; } = new Config();

        // Används när vi vill visa platser i UI
        public static IReadOnlyList<ParkingSpot> GetAllSpots() => _spots.AsReadOnly();

        public static void ApplyConfig(Config config)
        {
            config.Normalize();
            CurrentConfig = config;

            // Gör config tillgänglig för Car/MC osv.
            GarageSettings.Apply(config);
        }

        // Skapar en tom parkering enligt config (inkl. olika platsstorlekar)
        public static void ResetFromConfig()
        {
            _spots.Clear();

            for (int i = 1; i <= CurrentConfig.TotalSpots; i++)
            {
                int size = CurrentConfig.GetSpotSize(i);
                _spots.Add(new ParkingSpot(i, size));
            }
        }

        public static void LoadSpots(List<ParkingSpot> spots)
        {
            _spots = spots ?? new List<ParkingSpot>();
        }

        // Ser till att antal platser och kapacitet matchar config (utan att “förstöra” parkerade fordon)
        public static void EnsureGarageMatchesConfig()
        {
            int desired = CurrentConfig.TotalSpots;

            // Justera befintliga platser (kapacitet får aldrig bli < redan använd yta)
            for (int i = 0; i < _spots.Count; i++)
            {
                int spotNo = _spots[i].SpotNumber;
                int desiredSize = CurrentConfig.GetSpotSize(spotNo);

                if (_spots[i].GetUsedSpace() <= desiredSize)
                    _spots[i].Capacity = desiredSize;
            }

            // Lägg till nya tomma platser om config säger fler
            while (_spots.Count < desired)
            {
                int newSpotNo = _spots.Count + 1;
                int size = CurrentConfig.GetSpotSize(newSpotNo);
                _spots.Add(new ParkingSpot(newSpotNo, size));
            }

            // Ta bort tomma platser i slutet om config säger färre
            while (_spots.Count > desired && _spots.Last().IsEmpty())
                _spots.RemoveAt(_spots.Count - 1);
        }

        public static bool ParkVehicle(Vehicle vehicle)
        {
            // Först: försök plats som redan har samma typ (fyll upp smart)
            foreach (var spot in _spots)
            {
                if (!spot.IsEmpty() && spot.ParkedVehicles[0].GetType() == vehicle.GetType())
                {
                    if (spot.TryParkVehicle(vehicle, CurrentConfig))
                        return true;
                }
            }

            // Sen: hitta en tom plats
            foreach (var spot in _spots)
            {
                if (spot.IsEmpty() && spot.TryParkVehicle(vehicle, CurrentConfig))
                    return true;
            }

            return false;
        }

        // Flyttar fordon till en specifik plats (rollback om flytten misslyckas)
        public static bool MoveVehicle(string regNo, int targetSpotNumber)
        {
            if (targetSpotNumber < 1 || targetSpotNumber > _spots.Count)
                return false;

            int currentSpotNo = FindVehicle(regNo);
            if (currentSpotNo == -1)
                return false;

            if (currentSpotNo == targetSpotNumber)
                return false;

            Vehicle? vehicle = GetVehicle(regNo);
            if (vehicle == null)
                return false;

            var fromSpot = _spots[currentSpotNo - 1];
            var toSpot = _spots[targetSpotNumber - 1];

            // Ta bort först (annars blockar reglerna ofta)
            if (!fromSpot.RemoveVehicle(regNo))
                return false;

            // Försök parkera på target
            if (toSpot.TryParkVehicle(vehicle, CurrentConfig))
                return true;

            // Rollback om target inte går
            fromSpot.TryParkVehicle(vehicle, CurrentConfig);
            return false;
        }

        public static bool RemoveVehicle(string regNo)
        {
            foreach (var spot in _spots)
            {
                if (spot.RemoveVehicle(regNo))
                    return true;
            }
            return false;
        }

        public static int FindVehicle(string regNo)
        {
            regNo = (regNo ?? "").Trim().ToUpperInvariant();

            foreach (var spot in _spots)
            {
                if (spot.ParkedVehicles.Any(v => v.RegNo == regNo))
                    return spot.SpotNumber;
            }
            return -1;
        }

        public static Vehicle? GetVehicle(string regNo)
        {
            regNo = (regNo ?? "").Trim().ToUpperInvariant();

            foreach (var spot in _spots)
            {
                var v = spot.ParkedVehicles.FirstOrDefault(x => x.RegNo == regNo);
                if (v != null) return v;
            }
            return null;
        }

        public static List<Vehicle> GetAllVehicles()
            => _spots.SelectMany(s => s.ParkedVehicles).ToList();
    }
}
