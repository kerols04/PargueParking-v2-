using System.Collections.Generic;
using System.Linq;

namespace PragueParkingV2.Core
{
    public class ParkingSpot
    {
        public int SpotNumber { get; set; }
        public int Capacity { get; set; }
        public List<Vehicle> ParkedVehicles { get; set; } = new List<Vehicle>();

        public ParkingSpot() { }

        public ParkingSpot(int spotNumber, int capacity)
        {
            SpotNumber = spotNumber;
            Capacity = capacity;
        }

        // Försöker parkera ett fordon enligt regler och config
        public bool TryParkVehicle(Vehicle vehicle, Config config)
        {
            // Stoppar dubbla regnr
            if (ParkedVehicles.Any(v => v.RegNo == vehicle.RegNo))
                return false;

            // Tillåt bara en fordonstyp per plats (enkel och tydlig regel)
            if (ParkedVehicles.Count > 0 && ParkedVehicles[0].GetType() != vehicle.GetType())
                return false;

            string typeKey = vehicle.GetType().Name;
            int maxCount = config.GetVehicleType(typeKey)?.MaxPerSpot ?? 1;

            if (ParkedVehicles.Count >= maxCount)
                return false;

            if (GetAvailableSpace() < vehicle.Size)
                return false;

            ParkedVehicles.Add(vehicle);
            return true;
        }

        public bool RemoveVehicle(string regNo)
        {
            var found = ParkedVehicles.FirstOrDefault(v => v.RegNo == regNo);
            if (found == null) return false;

            ParkedVehicles.Remove(found);
            return true;
        }

        public bool IsEmpty() => ParkedVehicles.Count == 0;

        public int GetUsedSpace() => ParkedVehicles.Sum(v => v.Size);

        public int GetAvailableSpace() => Capacity - GetUsedSpace();

        public string GetStatusDescription()
        {
            if (IsEmpty()) return "Tom";
            if (GetAvailableSpace() == 0) return "Full";
            return "Delvis";
        }
    }
}

