namespace PragueParkingV2.Core
{
    public class Car : Vehicle
    {
        public Car() 
        {
            // Storlek hämtas från config
            Size = GarageSettings.GetVehicleSize(nameof(Car), 4);
        }

        public Car(string regNo) : base(regNo)
        {
            Size = GarageSettings.GetVehicleSize(nameof(Car), 4);
        }

        public override decimal GetHourlyRate() => PriceList.GetPrice(nameof(Car));

        public override string GetVehicleTypeName()
            => GarageSettings.GetDisplayName(nameof(Car), "Bil");
    }
}
