using System;

namespace PragueParkingV2.Core
{
    public class Car : Vehicle
    {
        public Car(string regNo) : base(regNo)
        {
            Size = GarageSettings.Current.GetVehicleSize("Car", fallback: 4);
        }

        public Car() : base()
        {
            Size = GarageSettings.Current.GetVehicleSize("Car", fallback: 4);
        }

        public override decimal GetHourlyRate() => PriceList.GetPrice("Car");

        public override string GetVehicleTypeName() => "Bil";
    }
}
