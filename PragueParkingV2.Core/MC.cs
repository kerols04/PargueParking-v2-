namespace PragueParkingV2.Core
{
    public class MC : Vehicle
    {
        public MC()
        {
            Size = GarageSettings.GetVehicleSize(nameof(MC), 2);
        }

        public MC(string regNo) : base(regNo)
        {
            Size = GarageSettings.GetVehicleSize(nameof(MC), 2);
        }

        public override decimal GetHourlyRate() => PriceList.GetPrice(nameof(MC));

        public override string GetVehicleTypeName()
            => GarageSettings.GetDisplayName(nameof(MC), "Motorcykel");
    }
}
