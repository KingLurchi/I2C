using System.Collections.Generic;
using System.Threading.Tasks;
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
        protected override string Name => nameof(Si7021);
        protected override int SlaveAddress => 0x40;
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

            var value = ReadRegister(Registers.MeasureRelativeHumidity, x => x[0] << 8 | x[1], 2);
            return 125.0 * value / 65536 - 6.0;
        }

        public async Task<double> GetTemperature()
        {
            await ConnectAndInitialize();

            var value = ReadRegister(Registers.MeasureTemperature, x => x[0] << 8 | x[1], 2);
            return 175.72 * value / 65536 - 46.85;
        }

        private static class Registers
        {
            public const byte MeasureRelativeHumidity = 0xE5;
            public const byte MeasureTemperature = 0xE3;
        }
    }
}