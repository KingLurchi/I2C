using System.Globalization;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using I2C.Core.Enums;
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
            while (true)
            {
                var altitude = await _bmp280.GetAltitude(1026.3f);
                Altitude.Text = altitude.ToString(CultureInfo.CurrentCulture);
                var pressure = await _bmp280.GetPressure();
                Pressure.Text = pressure.ToString(CultureInfo.CurrentCulture);
                var temperature = await _bmp280.GetTemperature();
                Temperature.Text = temperature.ToString(CultureInfo.CurrentCulture);

                var lux = await _tsl2591.GetLux(Gain.High, IntegrationTime.Long);
                Lux.Text = lux.ToString(CultureInfo.CurrentCulture);

                await Task.Delay(1000);
            }
        }
    }
}