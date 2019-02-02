using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.I2c;
using I2C.Core.Contracts;

namespace I2C.Core.Sensors
{
    public interface IVeml6070 : II2CDevice
    {
        Task<double> GetUv(Veml6070.IntegrationTime time = Veml6070.IntegrationTime.Short);
    }

    public class Veml6070 : BaseI2CDevice, IVeml6070
    {
        public enum IntegrationTime
        {
            Short = 1,
            Medium = 2,
            Long = 4,
            Longer = 8
        }

        public enum SlaveAddress
        {
            Ox38 = 0x38,
            Ox39 = 0x39
        }

        public Veml6070(I2cBusSpeed busSpeed = I2cBusSpeed.FastMode, I2cSharingMode sharingMode = I2cSharingMode.Shared, SlaveAddress slaveAddress = SlaveAddress.Ox39)
            : base(busSpeed, sharingMode, (int) slaveAddress)
        {
        }

        protected override string Name => nameof(Veml6070);

        protected override Dictionary<string, string> Wires => new Dictionary<string, string>
        {
            {"3V3", "VIN"},
            {"GND", "G"},
            {"SCL", "SCL"},
            {"SDA", "SDA"}
        };

        public async Task<double> GetUv(IntegrationTime time = IntegrationTime.Short)
        {
            await ConnectAndInitialize();

            await Delay(time);

            WriteRegister(Registers.AlertRespAddress, 0x01);

            var uv = ReadRegister(Registers.AddressHigh, x => x[0]);
            uv <<= 8;
            uv |= ReadRegister(Registers.AddressLow, x => x[0]);


            return uv;
        }

        private async Task Delay(IntegrationTime time)
        {
            await Task.Delay((int) Math.Pow(1, (int) time) * 63);
        }

        private static class Registers
        {
            public const byte AddressHigh = 0x39;
            public const byte AddressLow = 0x38;
            public const byte AlertRespAddress = 0x0C;
            public const byte Command = 0x02;
        }
    }
}