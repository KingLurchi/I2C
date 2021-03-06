﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.I2c;
using I2C.Core.Contracts;

namespace I2C.Core.Sensors
{
    public interface ISi7021 : II2CDevice
    {
        Task<double> GetHumidity();
        Task<double> GetTemperature();
    }

    public class Si7021 : BaseI2CDevice, ISi7021
    {
        public enum SlaveAddress
        {
            Ox40 = 0x40
        }

        public Si7021(I2cBusSpeed busSpeed = I2cBusSpeed.FastMode, I2cSharingMode sharingMode = I2cSharingMode.Shared, SlaveAddress slaveAddress = SlaveAddress.Ox40)
            : base(busSpeed, sharingMode, (int) slaveAddress)
        {
        }

        protected override string Name => nameof(Si7021);

        protected override Dictionary<string, string> Wires => new Dictionary<string, string>
        {
            {"3V3", "VIN"},
            {"GND", "GND"},
            {"SCL", "SCL"},
            {"SDA", "SDA"}
        };

        public async Task<double> GetHumidity()
        {
            await ConnectAndInitialize();

            var value = ReadRegister(Registers.MeasureRelativeHumidity, x => (x[0] << 8) | x[1], 2);
            return 125.0 * value / 65536 - 6.0;
        }

        public async Task<double> GetTemperature()
        {
            await ConnectAndInitialize();

            var value = ReadRegister(Registers.MeasureTemperature, x => (x[0] << 8) | x[1], 2);
            return 175.72 * value / 65536 - 46.85;
        }

        private static class Registers
        {
            public const byte MeasureRelativeHumidity = 0xE5;
            public const byte MeasureTemperature = 0xE3;
        }
    }
}