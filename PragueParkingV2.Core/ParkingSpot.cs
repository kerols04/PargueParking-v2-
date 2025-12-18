using System;
using System.Collections.Generic;
using System.Linq;

namespace PragueParkingV2.Core
{
    public class ParkingSpot
    {
        public int SpotNumber { get; set; }
        public int Capacity { get; set; }
        public List<Vehicle> ParkedVehicles { get; set; } = new();

        public ParkingSpot(int spotNumber, int capacity)
        {
            SpotNumber = spotNumber;
            Capacity = capacity;
        }

        public ParkingSpot() { }

        public int GetAvailableSpace() => Capacity - ParkedVehicles.Sum(v => v.Size);

        public bool TryParkVehicle(Vehicle vehicle)
        {
            if (vehicle == null) return false;

            string typeKey = vehicle.GetType().Name; // "Car" / "MC"
            int maxAllowed = GarageSettings.Current.GetMaxPerSpot(typeKey, fallback: int.MaxValue);

            int already = ParkedVehicles.Count(v => v.GetType().Name.Equals(typeKey, StringComparison.OrdinalIgnoreCase));
            if (already >= maxAllowed) return false;

            if (GetAvailableSpace() < vehicle.Size) return false;

            ParkedVehicles.Add(vehicle);
            return true;
        }

        public Vehicle RemoveVehicle(string regNo)
        {
            var vehicle = ParkedVehicles.FirstOrDefault(v => v.RegNo.Equals(regNo, StringComparison.OrdinalIgnoreCase));
            if (vehicle != null) ParkedVehicles.Remove(vehicle);
            return vehicle;
        }

        public bool HasVehicle(string regNo)
            => ParkedVehicles.Any(v => v.RegNo.Equals(regNo, StringComparison.OrdinalIgnoreCase));
    }
}
