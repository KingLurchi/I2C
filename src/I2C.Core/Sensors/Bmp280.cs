using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using I2C.Core.Contracts;

namespace I2C.Core.Sensors
{
    public interface IBmp280 : II2CDevice
    {
        Task<double> GetAltitude(double seaLevel);
        Task<double> GetPressure();
        Task<double> GetTemperature();
    }

    public sealed class Bmp280 : BaseI2CDevice, IBmp280
    {
        private CompensationData _compensationData;

        protected override byte IdentificationNumber => 0x58;
        protected override byte IdentificationRegister => Registers.Id;
        protected override string Name => nameof(Bmp280);
        protected override int SlaveAddress => 0x77;
        protected override Dictionary<string, string> Wires => new Dictionary<string, string>
        {
            {"3V3", "VIN"},
            {"GND", "GND"},
            {"SCL", "SCK"},
            {"SDA", "SDI"}
        };

        public async Task<double> GetAltitude(double seaLevel)
        {
            await ConnectAndInitialize();

            var pressure = (float) await GetPressure() / 100;
            return 44330.0f * (1.0f - (float)Math.Pow(pressure / seaLevel, 0.1903f));
        }

        public async Task<double> GetPressure()
        {
            await ConnectAndInitialize();

            var msb = ReadRegister(Registers.PressureMsb, x => x[0]);
            var lsb = ReadRegister(Registers.PressureLsb, x => x[0]);
            var xlsb = ReadRegister(Registers.PressureXlsb, x => x[0]);

            var raw = (msb << 12) + (lsb << 4) + (xlsb >> 4);

            var a = (long) GetFineTemperature() - 128000;
            var b = a * a * _compensationData.DigP6;
            b = b + ((a * _compensationData.DigP5) << 17);
            b = b + ((long)_compensationData.DigP4 << 35);
            a = ((a * a * _compensationData.DigP3) >> 8) + ((a * _compensationData.DigP2) << 12);
            a = ((((long)1 << 47) + a) * _compensationData.DigP1) >> 33;

            if (a == 0)
                return 0;

            var pressure = (long)1048576 - raw;
            pressure = ((pressure << 31) - b) * 3125 / a;

            a = (_compensationData.DigP9 * (pressure >> 13) * (pressure >> 13)) >> 25;
            b = (_compensationData.DigP8 * pressure) >> 19;

            pressure = ((pressure + a + b) >> 8) + ((long)_compensationData.DigP7 << 4);

            return pressure / 256.0;
        }

        public async Task<double> GetTemperature()
        {
            await ConnectAndInitialize();

            var fineTemperature = GetFineTemperature();
            return fineTemperature / 5120.0;
        }

        private double GetFineTemperature()
        {
            var msb = ReadRegister(Registers.TemperatureMsb, x => x[0]);
            var lsb = ReadRegister(Registers.TemperatureLsb, x => x[0]);
            var xlsb = ReadRegister(Registers.TemperatureXlsb, x => x[0]);

            var raw = (msb << 12) + (lsb << 4) + (xlsb >> 4);

            var a = (raw / 16384.0 - _compensationData.DigT1 / 1024.0) * _compensationData.DigT2;
            var b = (raw / 131072.0 - _compensationData.DigT1 / 8192.0) * _compensationData.DigT3;
            return a + b;
        }

        protected override void Setup()
        {
            _compensationData = GetCompensationData();
            SetControlRegister();
            SetHumidityControlRegister();
        }

        private CompensationData GetCompensationData()
        {
            return new CompensationData
            {
                DigT1 = ReadRegister(Registers.DigT1, x => (ushort)((x[1] << 8) + x[0]), 2),
                DigT2 = ReadRegister(Registers.DigT2, x => (short)((x[1] << 8) + x[0]), 2),
                DigT3 = ReadRegister(Registers.DigT3, x => (short)((x[1] << 8) + x[0]), 2),
                DigP1 = ReadRegister(Registers.DigP1, x => (ushort)((x[1] << 8) + x[0]), 2),
                DigP2 = ReadRegister(Registers.DigP2, x => (short)((x[1] << 8) + x[0]), 2),
                DigP3 = ReadRegister(Registers.DigP3, x => (short)((x[1] << 8) + x[0]), 2),
                DigP4 = ReadRegister(Registers.DigP4, x => (short)((x[1] << 8) + x[0]), 2),
                DigP5 = ReadRegister(Registers.DigP5, x => (short)((x[1] << 8) + x[0]), 2),
                DigP6 = ReadRegister(Registers.DigP6, x => (short)((x[1] << 8) + x[0]), 2),
                DigP7 = ReadRegister(Registers.DigP7, x => (short)((x[1] << 8) + x[0]), 2),
                DigP8 = ReadRegister(Registers.DigP8, x => (short)((x[1] << 8) + x[0]), 2),
                DigP9 = ReadRegister(Registers.DigP9, x => (short)((x[1] << 8) + x[0]), 2)
            };
        }

        private void SetControlRegister()
        {
            WriteRegister(Registers.Control, 0x3F);
        }

        private void SetHumidityControlRegister()
        {
            WriteRegister(Registers.HumidityControl, 0x03);
        }

        private static class Registers
        {
            public const byte Control = 0xF4;
            public const byte DigT1 = 0x88;
            public const byte DigT2 = 0x8A;
            public const byte DigT3 = 0x8C;
            public const byte DigP1 = 0x8E;
            public const byte DigP2 = 0x90;
            public const byte DigP3 = 0x92;
            public const byte DigP4 = 0x94;
            public const byte DigP5 = 0x96;
            public const byte DigP6 = 0x98;
            public const byte DigP7 = 0x9A;
            public const byte DigP8 = 0x9C;
            public const byte DigP9 = 0x9E;
            public const byte HumidityControl = 0xF2;
            public const byte Id = 0xD0;
            public const byte PressureMsb = 0xF7;
            public const byte PressureLsb = 0xF8;
            public const byte PressureXlsb = 0xF9;
            public const byte TemperatureMsb = 0xFA;
            public const byte TemperatureLsb = 0xFB;
            public const byte TemperatureXlsb = 0xFC;
        }

        private class CompensationData
        {
            public ushort DigT1 { get; set; }
            public short DigT2 { get; set; }
            public short DigT3 { get; set; }
            public ushort DigP1 { get; set; }
            public short DigP2 { get; set; }
            public short DigP3 { get; set; }
            public short DigP4 { get; set; }
            public short DigP5 { get; set; }
            public short DigP6 { get; set; }
            public short DigP7 { get; set; }
            public short DigP8 { get; set; }
            public short DigP9 { get; set; }
        }
    }
}