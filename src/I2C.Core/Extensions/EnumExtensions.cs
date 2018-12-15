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

        public static byte ToByte(this IntergrationTime time)
        {
            switch (time)
            {
                case IntergrationTime.Shortest:
                    return 0x00;
                case IntergrationTime.Shorter:
                    return 0x01;
                case IntergrationTime.Medium:
                    return 0x02;
                case IntergrationTime.Long:
                    return 0x03;
                case IntergrationTime.Longer:
                    return 0x04;
                case IntergrationTime.Longest:
                    return 0x05;
                default:
                    return 0x00;
            }
        }
    }
}