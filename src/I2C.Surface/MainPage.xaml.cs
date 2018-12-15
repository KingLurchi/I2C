﻿using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using I2C.Core.Sensors;

namespace I2C.Surface
{
    public sealed partial class MainPage : Page
    {
        private readonly IBmp280 _bmp280;
        private readonly ITsl2591 _tsl2591;

        public MainPage()
        {
            InitializeComponent();

            _bmp280 = new Bmp280();
            _tsl2591 = new Tsl2591();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var altitude = await _bmp280.GetAltitude(1026.3f);
            var pressure = await _bmp280.GetPressure();
            var temperature = await _bmp280.GetTemperature();

            var lux = await _tsl2591.GetLux();
        }
    }
}