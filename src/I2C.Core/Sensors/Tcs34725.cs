using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Windows.Devices.I2c;
using Windows.UI;
using I2C.Core.Contracts;
using I2C.Core.Extensions;

namespace I2C.Core.Sensors
{
    public interface ITcs34725 : II2CDevice
    {
        Task Configure(Tcs34725.Gain gain, Tcs34725.IntegrationTime time);
        Task<Color> GetRgb();
    }

    public class Tcs34725 : BaseI2CDevice, ITcs34725
    {
        public enum Gain
        {
            Low = 1,
            Medium = 4,
            High = 16,
            Maximum = 60
        }

        public enum IntegrationTime
        {
            Shortest = 4,
            Shorter = 24,
            Medium = 101,
            Long = 154,
            Longer = 700
        }

        public enum SlaveAddress
        {
            Ox29 = 0x29
        }

        private int _gain = (int) Gain.Low;
        private int _integrationTime = (int) IntegrationTime.Shortest;

        public Tcs34725(I2cBusSpeed busSpeed = I2cBusSpeed.FastMode, I2cSharingMode sharingMode = I2cSharingMode.Shared, SlaveAddress slaveAddress = SlaveAddress.Ox29)
            : base(busSpeed, sharingMode, (int)slaveAddress)
        {
        }

        protected override byte IdentificationNumber => 0x44;
        protected override byte? IdentificationRegister => Registers.Command | Registers.Id;
        protected override string Name => nameof(Tcs34725);

        protected override Dictionary<string, string> Wires => new Dictionary<string, string>
        {
            {"3V3", "VIN"},
            {"GND", "GND"},
            {"SCL", "SCL"},
            {"SDA", "SDA"}
        };

        public async Task Configure(Gain gain, IntegrationTime time)
        {
            await ConnectAndInitialize();

            _gain = (int) gain;
            WriteRegister(Registers.Command | Registers.Oscillator, gain.ToByte());

            _integrationTime = (int) time;
            WriteRegister(Registers.Command | Registers.Oscillator, time.ToByte());
        }

        public async Task<Color> GetRgb()
        {
            await ConnectAndInitialize();
            await Enable();

            var clear = ReadRegister(Registers.Command | Registers.CDataL, x => (x[1] << 8) | x[0], 2);
            var red = ReadRegister(Registers.Command | Registers.RDataL, x => (x[1] << 8) | x[0], 2);
            var green = ReadRegister(Registers.Command | Registers.GDataL, x => (x[1] << 8) | x[0], 2);
            var blue = ReadRegister(Registers.Command | Registers.BDataL, x => (x[1] << 8) | x[0], 2);

            await Task.Delay(_integrationTime);

            Disable();

            var color = clear == 0 ? Color.FromArgb(0, 0, 0, 0) : Color.FromArgb(255, ClampValue(red, clear), ClampValue(green, clear), ClampValue(red, blue));

            Debug.WriteLine($"Getting rgb value of {color.ToString(CultureInfo.CurrentCulture)} with gain {_gain} and integration time {_integrationTime}");

            return color;
        }

        private static byte ClampValue(int color, int clear)
        {
            return (byte) ((float) color / clear * 255.0);
        }

        private void Disable()
        {
            var enable = ReadRegister(Registers.Command | Registers.Enable, x => x[0]);
            WriteRegister(Registers.Command | Registers.Enable, (byte) (enable & (Registers.Oscillator | Registers.Adc)));
        }

        private async Task Enable()
        {
            WriteRegister(Registers.Command | Registers.Enable, Registers.Oscillator);
            WriteRegister(Registers.Command | Registers.Enable, Registers.Oscillator | Registers.Adc);
            await Task.Delay(_integrationTime);
        }

        private static class Registers
        {
            public const byte Adc = 0x02;
            public const byte BDataL = 0x1A;
            public const byte CDataL = 0x14;
            public const byte Command = 0x80;
            public const byte Enable = 0x00;
            public const byte GDataL = 0x18;
            public const byte Id = 0x12;
            public const byte RDataL = 0x16;
            public const byte Oscillator = 0x01;
        }
    }
}