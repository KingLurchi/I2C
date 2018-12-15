using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using I2C.Core.Contracts;

namespace I2C.Core.Sensors
{
    public interface ITsl2591 : II2CDevice
    {
        Task<double> GetLux();
    }

    public sealed class Tsl2591 : BaseI2CDevice, ITsl2591
    {
        protected override byte IdentificationNumber => 0x12;
        protected override byte IdentificationRegister => 0x12;
        protected override string Name => nameof(Tsl2591);
        protected override int SlaveAddress => 0x29;
        protected override Dictionary<string, string> Wires => new Dictionary<string, string>
        {
            {"3V3", "VIN"},
            {"GND", "GND"},
            {"SCL", "SCL"},
            {"SDA", "SDA"}
        };

        public async Task<double> GetLux()
        {
            await ConnectAndInitialize();

            await Task.Delay(5000);

            var channel0 = ReadRegister(Registers.C0DataL | Registers.Command, x => (ushort) ((x[1] << 8) | x[0]), 2);
            var channel1 = ReadRegister(Registers.C1DataL | Registers.Command, x => (ushort) ((x[1] << 8) | x[0]), 2);

            if (channel0 == 0xFFFF || channel1 == 0xFFFF)
                return 0.0;

            var gain = GetGain(0x10);
            var time = (0x01 + 1) * 100;

            var cpl = gain * time / 408.0;
            var lux1 = (channel0 - 1.64 * channel1) / cpl;
            var lux2 = (0.59 * channel0 - 0.86 * channel1) / cpl;

            return Math.Round(Math.Max(lux1, lux2), 4);
        }

        private double GetGain(uint gain)
        {
            switch (gain)
            {
                case 0x10:
                    return 25;
                case 0x20:
                    return 428;
                case 0x30:
                    return 9876;
                default:
                    return 1;
            }
        }

        protected override void Setup()
        {
            TurnOn();
            SetGain();
        }

        private void TurnOn()
        {
            WriteRegister(Registers.Enable | Registers.Command, 0x03);
        }

        private void SetGain()
        {
            WriteRegister(Registers.Config, 0x10 + 0x01);
        }

        private static class Registers
        {
            public const byte C0DataL= 0x14;
            public const byte C1DataL= 0x16;
            public const byte Command = 0xA0;
            public const byte Config = 0x01;
            public const byte Enable = 0x00;
        }
    }
}