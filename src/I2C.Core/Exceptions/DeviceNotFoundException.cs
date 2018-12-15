using System;

namespace I2C.Core.Exceptions
{
    public class DeviceNotFoundException : Exception
    {
        public DeviceNotFoundException(string device) : base($"A connected {device} sensor could not be found.")
        {
        }
    }
}