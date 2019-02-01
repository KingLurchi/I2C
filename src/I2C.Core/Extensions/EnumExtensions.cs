namespace I2C.Core.Extensions
{
    public static class EnumExtensions
    {
        public static byte ToByte(this Sensors.Tsl2591.Gain gain)
        {
            switch (gain)
            {
                case Sensors.Tsl2591.Gain.Low:
                    return 0x00;
                case Sensors.Tsl2591.Gain.Medium:
                    return 0x10;
                case Sensors.Tsl2591.Gain.High:
                    return 0x20;
                case Sensors.Tsl2591.Gain.Maximum:
                    return 0x30;
                default:
                    return 0x00;
            }
        }

        public static byte ToByte(this Sensors.Tsl2591.IntegrationTime time)
        {
            switch (time)
            {
                case Sensors.Tsl2591.IntegrationTime.Shortest:
                    return 0x00;
                case Sensors.Tsl2591.IntegrationTime.Shorter:
                    return 0x01;
                case Sensors.Tsl2591.IntegrationTime.Medium:
                    return 0x02;
                case Sensors.Tsl2591.IntegrationTime.Long:
                    return 0x03;
                case Sensors.Tsl2591.IntegrationTime.Longer:
                    return 0x04;
                case Sensors.Tsl2591.IntegrationTime.Longest:
                    return 0x05;
                default:
                    return 0x00;
            }
        }
    }
}