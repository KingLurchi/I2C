using System;

namespace I2C.Core.Exceptions
{
    public class IdMismatchException : Exception
    {
        public IdMismatchException(string device, byte expectedId, byte actualId) : base($"The identification number {actualId} does not match the expected number of {expectedId} for a {device} sensor.")
        {
        }
    }
}