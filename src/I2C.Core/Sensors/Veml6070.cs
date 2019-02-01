using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using I2C.Core.Contracts;
using IntegrationTime = I2C.Core.Sensors.Veml6070.IntegrationTime;

namespace I2C.Core.Sensors
{
    public interface IVeml6070 : II2CDevice
    {
        Task<double> GetUv(IntegrationTime time = IntegrationTime.Short);
    }

    public class Veml6070 : BaseI2CDevice, IVeml6070
    {
        protected override string Name => nameof(Veml6070);
        protected override int SlaveAddress => 0x39; // 0x39
        protected override Dictionary<string, string> Wires => new Dictionary<string, string>
        {
            {"3V3", "VIN"},
            {"GND", "GND"},
            {"SCL", "SCK"},
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
            await Task.Delay((int)Math.Pow(1, (int) time) * 63);
        }

        private static class Registers
        {
            public const byte AddressHigh = 0x39;
            public const byte AddressLow = 0x38;
            public const byte AlertRespAddress = 0x0C;
            public const byte Command = 0x02;
        }

        public enum IntegrationTime
        {
            Short = 1,
            Medium = 2,
            Long = 4,
            Longer = 8
        }
    }
}