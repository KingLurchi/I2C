using System.Collections.Generic;
using I2C.Core.Contracts;

namespace I2C.Core.Sensors
{
    public interface ITcs34725 : II2CDevice
    {
    }

    public class Tcs34725 : BaseI2CDevice, ITcs34725
    {
        protected override string Name => nameof(Tcs34725);
        protected override int SlaveAddress => 0x29;
        protected override Dictionary<string, string> Wires => new Dictionary<string, string>
        {
            {"3V3", "VIN"},
            {"GND", "GND"},
            {"SCL", "SCL"},
            {"SDA", "SDA"}
        };
    }
}