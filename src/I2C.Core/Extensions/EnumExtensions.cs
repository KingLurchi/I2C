using I2C.Core.Enums;

namespace I2C.Core.Extensions
{
    public static class EnumExtensions
    {
        public static byte ToByte(this Gain gain)
        {
            switch (gain)
            {
                case Gain.Low:
                    return 0x00;
                case Gain.Medium:
                    return 0x10;
                case Gain.High:
                    return 0x20;
                case Gain.Maximum:
                    return 0x30;
                default:
                    return 0x00;
            }
        }

        public static byte ToByte(this IntegrationTime time)
        {
            switch (time)
            {
                case IntegrationTime.Shortest:
                    return 0x00;
                case IntegrationTime.Shorter:
                    return 0x01;
                case IntegrationTime.Medium:
                    return 0x02;
                case IntegrationTime.Long:
                    return 0x03;
                case IntegrationTime.Longer:
                    return 0x04;
                case IntegrationTime.Longest:
                    return 0x05;
                default:
                    return 0x00;
            }
        }
    }
}