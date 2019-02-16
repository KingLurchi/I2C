using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.I2c;
using Windows.UI.Xaml.Controls;
using I2C.Core.Contracts;

namespace I2C.Core.Sensors
{
    public interface ISsd1306 : II2CDevice
    {
        Task Display(Image image);
        Task Display(string message);
    }

    public class Ssd1306 : BaseI2CDevice, ISsd1306
    {
        public enum SlaveAddress
        {
            Ox3C = 0x3C
        }

        public Ssd1306(I2cBusSpeed busSpeed = I2cBusSpeed.FastMode, I2cSharingMode sharingMode = I2cSharingMode.Shared, SlaveAddress slaveAddress = SlaveAddress.Ox3C) 
            : base(busSpeed, sharingMode, (int)slaveAddress)
        {
        }

        protected override string Name => nameof(Ssd1306);

        protected override Dictionary<string, string> Wires => new Dictionary<string, string>
        {
            {"3V3", "VIN"},
            {"GND", "GND"},
            {"SCL", "SCL"},
            {"SDA", "SDA"}
        };

        public async Task Display(Image image)
        {
            await ConnectAndInitialize();

            //TODO: display image
        }

        public async Task Display(string message)
        {
            await ConnectAndInitialize();

            //TODO: display message
        }
    }
}