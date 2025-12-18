using System;
using System.Collections.Generic;
using System.Linq;

namespace PragueParkingV2.Core
{
    public static class PHus
    {
        private static List<ParkingSpot> parkingSpots = new();

        static PHus()
        {
            ResetSpots(100);
        }

        public static void ResetSpots(int numberOfSpots)
        {
            if (numberOfSpots <= 0) numberOfSpots = 100;

            parkingSpots = new List<ParkingSpot>();

            for (int i = 1; i <= numberOfSpots; i++)
            {
                int capacity = GarageSettings.Current.GetSpotSize(i);
                parkingSpots.Add(new ParkingSpot(i, capacity));
            }
        }

        public static void LoadSpots(List<ParkingSpot> spots)
        {
            if (spots == null || spots.Count == 0) return;
            parkingSpots = spots;
        }

        public static List<ParkingSpot> GetAllSpots() => parkingSpots;

        public static List<Vehicle> GetAllVehicles()
            => parkingSpots.SelectMany(s => s.ParkedVehicles).ToList();

        public static bool ParkVehicle(Vehicle vehicle)
        {
            if (vehicle == null) return false;

            var orderedSpots = parkingSpots
                .OrderByDescending(s => s.ParkedVehicles.Any(v => v is MC))
                .ThenByDescending(s => s.GetAvailableSpace());

            foreach (var spot in orderedSpots)
            {
                if (spot.TryParkVehicle(vehicle))
                    return true;
            }

            return false;
        }

        public static Vehicle RetrieveVehicle(string regNo)
        {
            if (string.IsNullOrWhiteSpace(regNo)) return null;

            foreach (var spot in parkingSpots)
            {
                if (spot.HasVehicle(regNo))
                    return spot.RemoveVehicle(regNo);
            }
            return null;
        }

        public static bool MoveVehicle(string regNo)
        {
            var fromSpot = FindSpotNumber(regNo);
            var vehicle = RetrieveVehicle(regNo);
            if (vehicle == null) return false;

            bool moved = ParkVehicle(vehicle);

            if (!moved)
            {
                if (fromSpot.HasValue)
                {
                    var original = parkingSpots.FirstOrDefault(s => s.SpotNumber == fromSpot.Value);
                    if (original != null && original.TryParkVehicle(vehicle))
                        return false;
                }

                ParkVehicle(vehicle);
            }

            return moved;
        }

        public static bool MoveVehicle(string regNo, int targetSpotNumber)
        {
            if (targetSpotNumber <= 0) return false;

            var fromSpot = FindSpotNumber(regNo);
            var vehicle = RetrieveVehicle(regNo);
            if (vehicle == null) return false;

            var target = parkingSpots.FirstOrDefault(s => s.SpotNumber == targetSpotNumber);
            if (target != null && target.TryParkVehicle(vehicle))
                return true;

            if (fromSpot.HasValue)
            {
                var original = parkingSpots.FirstOrDefault(s => s.SpotNumber == fromSpot.Value);
                if (original != null && original.TryParkVehicle(vehicle))
                    return false;
            }

            ParkVehicle(vehicle);
            return false;
        }

        public static Vehicle FindVehicle(string regNo)
            => parkingSpots.SelectMany(s => s.ParkedVehicles)
                .FirstOrDefault(v => v.RegNo.Equals(regNo, StringComparison.OrdinalIgnoreCase));

        public static int? FindSpotNumber(string regNo)
        {
            var spot = parkingSpots.FirstOrDefault(s => s.ParkedVehicles.Any(v =>
                v.RegNo.Equals(regNo, StringComparison.OrdinalIgnoreCase)));

            return spot?.SpotNumber;
        }

        public static int CountEmptySpots() => parkingSpots.Count(s => s.ParkedVehicles.Count == 0);

        public static int CountFullSpots() => parkingSpots.Count(s => s.GetAvailableSpace() == 0);

        public static int CountPartialSpots()
            => parkingSpots.Count(s => s.ParkedVehicles.Count > 0 && s.GetAvailableSpace() > 0);
    }
}
