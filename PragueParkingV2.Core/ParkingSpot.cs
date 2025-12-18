using System;
using System.Collections.Generic;
using System.Linq;

namespace PragueParkingV2.Core
{
    // Representerar en enda parkeringsruta.
    public class ParkingSpot
    {
        public int SpotNumber { get; set; }         // Platsnummer (1..N)
        public int Capacity { get; set; }           // Total storlek i enheter (t.ex. 4)
        public List<Vehicle> ParkedVehicles { get; set; } = new List<Vehicle>();

        public ParkingSpot(int spotNumber, int capacity = 4)
        {
            SpotNumber = spotNumber;
            Capacity = capacity;
        }

        public ParkingSpot() { }

        // Ledigt utrymme i rutan just nu.
        public int GetAvailableSpace()
        {
            int usedSpace = ParkedVehicles.Sum(v => v.Size);
            return Capacity - usedSpace;
        }

        public bool IsEmpty() => ParkedVehicles.Count == 0;

        // "Full" enligt reglerna i config (t.ex. 2 MC = full även om Capacity har enheter kvar).
        public bool IsFull(Config config)
        {
            if (IsEmpty()) return false;

            config ??= new Config();

            // Vi tillåter inte mix av typer på samma ruta (matchar klassiska reglerna).
            string typeKey = ParkedVehicles[0].GetType().Name;

            int maxCount = int.MaxValue;
            if (config.MaxVehiclesPerSpot != null &&
                config.MaxVehiclesPerSpot.TryGetValue(typeKey, out int configuredMax))
            {
                maxCount = configuredMax;
            }

            return ParkedVehicles.Count >= maxCount || GetAvailableSpace() <= 0;
        }

        // Försöker parkera ett nytt fordon här (med config-regler).
        public bool TryParkVehicle(Vehicle vehicle, Config config)
        {
            if (vehicle == null) return false;

            config ??= new Config();

            string typeKey = vehicle.GetType().Name;

            // 1) Tillåten typ?
            if (config.VehicleTypes != null && config.VehicleTypes.Count > 0)
            {
                bool allowed = config.VehicleTypes.Any(t =>
                    t.Equals(typeKey, StringComparison.OrdinalIgnoreCase));

                if (!allowed) return false;
            }

            // 2) Inga blandade typer på samma ruta (Bil + MC ihop = nej).
            if (!IsEmpty())
            {
                string existingType = ParkedVehicles[0].GetType().Name;
                if (!existingType.Equals(typeKey, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            // 3) Max antal per plats för just den här typen.
            if (config.MaxVehiclesPerSpot != null &&
                config.MaxVehiclesPerSpot.TryGetValue(typeKey, out int max) &&
                ParkedVehicles.Count >= max)
            {
                return false;
            }

            // 4) Finns fysisk plats kvar i enheter?
            if (vehicle.Size > GetAvailableSpace())
                return false;

            ParkedVehicles.Add(vehicle);
            return true;
        }

        // Tar bort fordonet (om det finns) och returnerar det.
        public Vehicle RemoveVehicle(string regNo)
        {
            var vehicle = ParkedVehicles.FirstOrDefault(v =>
                v.RegNo.Equals(regNo, StringComparison.OrdinalIgnoreCase));

            if (vehicle == null) return null;

            ParkedVehicles.Remove(vehicle);
            return vehicle;
        }

        public string GetStatusDescription(Config config)
        {
            if (IsEmpty()) return "Tom";
            if (IsFull(config)) return "Full";
            return $"Delvis ({ParkedVehicles.Count} fordon)";
        }
    }
}
