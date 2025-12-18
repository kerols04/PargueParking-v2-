using System;

namespace PragueParkingV2.Core
{
    public class MC : Vehicle
    {
        public MC(string regNo) : base(regNo)
        {
            Size = GarageSettings.Current.GetVehicleSize("MC", fallback: 1);
        }

        public MC() : base()
        {
            Size = GarageSettings.Current.GetVehicleSize("MC", fallback: 1);
        }

        public override decimal GetHourlyRate() => PriceList.GetPrice("MC");

        public override string GetVehicleTypeName() => "Motorcykel";
    }
}
