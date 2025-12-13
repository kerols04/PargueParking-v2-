using System;

namespace PragueParkingV2.Core
{
    // Den här klassen är den viktigaste, alla fordon bygger på den. 
    // Den är 'abstract' för att den inte kan användas direkt, utan måste ärvas.
    public abstract class Vehicle
    {
        // Detaljer som alla fordon måste ha
        public string RegNo { get; set; }
        public int Size { get; protected set; } // Hur stor plats fordonet tar
        public DateTime CheckInTime { get; set; } // När fordonet körde in

        // Den här funktionen MÅSTE alla klasser som ärver från Vehicle skriva själva.
        public abstract decimal GetHourlyRate();

        // Så här skapas ett fordon i grunden
        protected Vehicle(string regNo)
        {
            RegNo = regNo.ToUpper(); // Gör regnumret till stora bokstäver
            CheckInTime = DateTime.Now; // Sätter incheckningstiden till nu
        }
        // En tom konstruktor som behövs för att JSON-hanteringen ska kunna läsa in data från filen
        protected Vehicle()
        {
        }
        // Räknar ut hur länge fordonet har stått parkerat
        public TimeSpan GetParkingDuration()
        {
            return DateTime.Now - CheckInTime;
        }
        // Räknar ut vad hela parkeringen kostar
        public decimal CalculateFee()
        {
            TimeSpan duration = GetParkingDuration();

            // Kollar om parkeringstiden är kortare än gratisperioden (t.ex. 10 minuter)
            if (duration.TotalMinutes <= PriceList.FreeMinutes)
            {
                return 0; // Det är gratis!
            }
            // Räknar ut hur många timmar det ska betalas för (avrundar alltid uppåt)
            int hours = (int)Math.Ceiling(duration.TotalHours);

            // Använder priset som är specifikt för denna fordonstyp (Bil eller MC)
            return hours * GetHourlyRate();
        }
        // Ger ett snyggt namn på fordonstypen som kan visas för användaren i menyn
        public virtual string GetVehicleTypeName()
        {
            return this.GetType().Name;
        }
    }
}