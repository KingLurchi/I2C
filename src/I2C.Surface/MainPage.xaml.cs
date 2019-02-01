using System;
using System.Globalization;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using I2C.Core.Sensors;

namespace I2C.Surface
{
    public sealed partial class MainPage : Page
    {
        private readonly IBmp280 _bmp280;
        private readonly ISi7021 _si7021;
        private readonly IVeml6070 _veml6070;
        private readonly ITcs34725 _tcs34725;
        private readonly ITsl2591 _tsl2591;

        public MainPage()
        {
            InitializeComponent();

            _bmp280 = new Bmp280();
            _si7021 = new Si7021();
            _veml6070 = new Veml6070();
            _tcs34725 = new Tcs34725();
            _tsl2591 = new Tsl2591();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                await SensorLoop();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }

        }

        private async Task SensorLoop()
        {
            await _tcs34725.Configure(Tcs34725.Gain.High, Tcs34725.IntegrationTime.Medium);

            while (true)
            {
                var altitude = await _bmp280.GetAltitude(1026.3f);
                Altitude.Text = altitude.ToString(CultureInfo.CurrentCulture);
                var pressure = await _bmp280.GetPressure();
                Pressure.Text = pressure.ToString(CultureInfo.CurrentCulture);
                var temperature = await _bmp280.GetTemperature();
                Temperature.Text = temperature.ToString(CultureInfo.CurrentCulture);

                var humidity = await _si7021.GetHumidity();
                Humidity.Text = humidity.ToString(CultureInfo.CurrentCulture);
                var temperature2 = await _si7021.GetTemperature();
                Temperature2.Text = temperature2.ToString(CultureInfo.CurrentCulture);

                var uv = await _veml6070.GetUv();
                Uv.Text = uv.ToString(CultureInfo.CurrentCulture);

                var rgb = await _tcs34725.GetRgb();
                Color.Foreground = new SolidColorBrush(rgb);
                Color.Text = rgb.ToString(CultureInfo.CurrentCulture);

                var lux = await _tsl2591.GetLux(Tsl2591.Gain.High, Tsl2591.IntegrationTime.Long);
                Lux.Text = lux.ToString(CultureInfo.CurrentCulture);

                await Task.Delay(1000);
            }
        }
    }
}