using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PragueParkingV2.Core;

namespace PragueParkingV2.Tests
{
    [TestClass]
    public class VehicleTests
    {
        [TestInitialize]
        public void Setup()
        {
            PriceList.LoadPrices();
        }

        [TestMethod]
        public void Car_Should_Calculate_Correct_Fee_After_Two_Hours()
        {
            var car = new Car("ABC123")
            {
                CheckInTime = DateTime.Now.AddHours(-2)
            };

            decimal fee = car.CalculateFee();

            Assert.AreEqual(40m, fee); // 2h * 20 kr/h
        }

        [TestMethod]
        public void MC_Should_Be_Free_Within_FreeMinutes()
        {
            var mc = new MC("XYZ999")
            {
                CheckInTime = DateTime.Now.AddMinutes(-5)
            };

            decimal fee = mc.CalculateFee();

            Assert.AreEqual(0m, fee);
        }
    }
}
