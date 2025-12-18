using System;

namespace PragueParkingV2.Core
{
    public abstract class Vehicle
    {
        private string _regNo = "";

        // Regnr normaliseras så sök/park alltid matchar
        public string RegNo
        {
            get => _regNo;
            set => _regNo = (value ?? "").Trim().ToUpperInvariant();
        }

        // Public set behövs för JSON (ladda/spara)
        public int Size { get; set; }

        public DateTime CheckInTime { get; set; } = DateTime.Now;

        protected Vehicle() { }

        protected Vehicle(string regNo)
        {
            RegNo = regNo;
            CheckInTime = DateTime.Now;
        }

        public decimal CalculateFee()
        {
            TimeSpan duration = DateTime.Now - CheckInTime;

            // Gratis minuter från prislistan
            if (duration.TotalMinutes <= PriceList.FreeMinutes)
                return 0;

            double hours = Math.Ceiling(duration.TotalHours);
            return (decimal)hours * GetHourlyRate();
        }

        public abstract decimal GetHourlyRate();
        public abstract string GetVehicleTypeName();
    }
}
