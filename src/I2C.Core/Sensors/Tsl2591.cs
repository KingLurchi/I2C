using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using I2C.Core.Contracts;
using I2C.Core.Enums;
using I2C.Core.Extensions;

namespace I2C.Core.Sensors
{
    public interface ITsl2591 : II2CDevice
    {
        Task<double> GetLux(Gain gain = Gain.Low, IntergrationTime time = IntergrationTime.Shortest);
    }

    public sealed class Tsl2591 : BaseI2CDevice, ITsl2591
    {
        protected override byte IdentificationNumber => 0x50;
        protected override byte IdentificationRegister => Registers.Command | Registers.Id;
        protected override string Name => nameof(Tsl2591);
        protected override int SlaveAddress => 0x29;
        protected override Dictionary<string, string> Wires => new Dictionary<string, string>
        {
            {"3V3", "VIN"},
            {"GND", "GND"},
            {"SCL", "SCL"},
            {"SDA", "SDA"}
        };

        public async Task<double> GetLux(Gain gain = Gain.Low, IntergrationTime time = IntergrationTime.Shortest)
        {
            await ConnectAndInitialize();

            Configure(gain, time);

            Enable();

            await Task.Delay((int)time);

            var y = ReadRegister(Registers.Command | Registers.C0DataL, x => (ushort)((x[1] << 8) | x[0]), 2) | 0;
            var luminosity = (ReadRegister(Registers.Command | Registers.C1DataL, x => (ushort)((x[1] << 8) | x[0]), 2) << 16) | y;

            Disable();

            var full = luminosity & 0xFFFF;
            var ir = luminosity >> 16;

            if (full == 0xFFFF | ir == 0xFFFF)
                return 0.0;

            var cpl = (float)gain * (float)time / 408.0F;
            return (full - ir) * (1.0f - ir/full) / cpl;
        }

        private void Disable()
        {
            WriteRegister(Registers.Command | Registers.Enable, 0x00);
        }

        private void Enable()
        {
            WriteRegister(Registers.Command | Registers.Enable, 0x01 | 0x02 | 0x10 | 0x80);
        }

        private void Configure(Gain gain, IntergrationTime time)
        {
            WriteRegister(Registers.Command | Registers.Control, (byte)(gain.ToByte() | time.ToByte()));
        }

        private static class Registers
        {
            public const byte C0DataL= 0x14;
            public const byte C1DataL= 0x16;
            public const byte Command = 0xA0;
            public const byte Control = 0x01;
            public const byte Enable = 0x00;
            public const byte Id = 0x12;
        }
    }
}