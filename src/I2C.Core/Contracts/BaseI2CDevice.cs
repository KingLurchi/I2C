using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using I2C.Core.Exceptions;

namespace I2C.Core.Contracts
{
    public abstract class BaseI2CDevice
    {
        protected abstract byte IdentificationNumber { get; }
        protected abstract byte IdentificationRegister { get; }
        protected abstract string Name { get; }
        protected abstract int SlaveAddress { get; }
        protected abstract Dictionary<string, string> Wires { get; }

        protected I2cDevice Device { get; private set; }

        private bool IsConnected { get; set; }
        private bool IsInitialized { get; set; }

        /// <summary>
        ///     Tries to establish a connection to the device and initializes it. Call this method first in every public
        ///     method of the derived class to ensure device is ready to be used.
        /// </summary>
        protected async Task ConnectAndInitialize()
        {
            await Connect();
            await Initialize();
        }

        private async Task Connect()
        {
            try
            {
                if (IsConnected)
                    return;

                var devices = await DeviceInformation.FindAllAsync(I2cDevice.GetDeviceSelector(Constants.I2CControllerName));
                if (devices.Count == 0)
                    throw new DeviceNotFoundException(Name);

                var connectionSettings = new I2cConnectionSettings(SlaveAddress)
                {
                    BusSpeed = I2cBusSpeed.FastMode,
                    SharingMode = I2cSharingMode.Shared
                };

                Device = await I2cDevice.FromIdAsync(devices[0].Id, connectionSettings);
                if (Device == null)
                    throw new DeviceNotFoundException(Name);

                IsConnected = true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }

        private async Task Initialize()
        {
            if (IsInitialized)
                return;

            var identificationNumber = ReadRegister(IdentificationRegister, x => x[0]);
            if (identificationNumber != IdentificationNumber)
                throw new IdMismatchException(Name, IdentificationNumber, identificationNumber);

            Setup();
            await SetupAsync();

            IsInitialized = true;
        }

        /// <summary>
        ///     Reads data from a register and converts it.
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="writeByte">The data written to the device</param>
        /// <param name="func">A func to convert data from the device into the correct format</param>
        /// <param name="bytes">The read buffers size in bytes</param>
        /// <returns>The read and converted data</returns>
        protected T ReadRegister<T>(byte writeByte, Func<byte[], T> func, int bytes = 1)
        {
            var writeBuffer = new[] {writeByte};
            var readBuffer = new byte[bytes];

            Device.WriteRead(writeBuffer, readBuffer);
            return func.Invoke(readBuffer);
        }

        /// <summary>
        ///     Override this method if additional work has to be done in order to properly initialize the device. 
        /// </summary>
        protected virtual void Setup()
        {

        }

        /// <summary>
        ///     Override this method if additional asynchronous work has to be done in order to properly initialize the device.
        /// </summary>
        protected virtual Task SetupAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Prints all necessary wiring information. This method works without an actual sensor present.
        /// </summary>
        public void Wiring()
        {
            if (Wires.Count == 0)
            {
                Debug.WriteLine($"No wiring information is provided for the {Name} sensor.");
                return;
            }

            Debug.WriteLine($"Wiring information to connect a rasbperry pi and a {Name} sensor:");
            Debug.WriteLine("-----------------------------------------------------------------");

            foreach (var wire in Wires)
                Debug.WriteLine($"Rasbperry Pi '{wire.Key}' to sensor '{wire.Value}'");

            Debug.WriteLine("-----------------------------------------------------------------");
        }

        /// <summary>
        ///     Write data to a register.
        /// </summary>
        /// <param name="register">The registers address</param>
        /// <param name="data">The value to write</param>
        protected void WriteRegister(byte register, byte data)
        {
            var buffer = new[] {register, data};
            Device.Write(buffer);
        }
    }
}