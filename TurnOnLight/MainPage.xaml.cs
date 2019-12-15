using System;
using System.Collections.Generic;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;



// Il modello di elemento Pagina vuota è documentato all'indirizzo https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x410

namespace TurnOnLight
{
    /// <summary>
    /// Pagina d'avvio dell'applicazione
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // GPO5 = PIN29 ON EXPANSION BOARD
        // ATTENTION PLEASE: Connect the longer leg of the led to the second resistor.
        private const int LedGreenPin = 5;

        // GPO6 = PIN31 ON EXPANSION BOARD
        // ATTENTION PLEASE: Connect the longer leg of the led to the second resistor.
        private const int LedYellowPin = 6;


        // office
        private GpioPin _pinGreen;

        // kitchen
        private GpioPin _pinYellow;

        public MainPage()
        {
            InitializeComponent();
            InitGpio();
        }

        /// <summary>
        /// Inizializzazione dei GPIO
        /// </summary>
        private void InitGpio()
        {
            GpioController gpio = GpioController.GetDefault();

            _pinGreen = gpio.OpenPin(LedGreenPin);
            _pinYellow = gpio.OpenPin(LedYellowPin);
            _pinGreen.SetDriveMode(GpioPinDriveMode.Output);
            _pinYellow.SetDriveMode(GpioPinDriveMode.Output);
        }

        /// <summary>
        /// Agisce sul GPIO associato alla giusta entità: ufficio, cucina, tutta la casa.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private IEnumerable<GpioPin> SelectedPin(Entity entity)
        {
            List<GpioPin> result = new List<GpioPin>();
            switch (entity)
            {
                case Entity.Green:
                    LightAccess(result, entity, _pinGreen);
                    break;
                case Entity.Yellow:
                    LightAccess(result, entity, _pinYellow);
                    break;
                case Entity.AllLights:
                    LightAccess(result, Entity.Green, _pinGreen);
                    LightAccess(result, Entity.Yellow, _pinYellow);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(entity), entity, null);
            }

            return result.ToArray();
        }

        private void LightAccess(IList<GpioPin> pinList, Entity light, GpioPin pin)
        {
            pinList.Add(pin);
        }

        /// <summary>
        /// Dopo l'input dell'utente avvia la connessione e dopo la risposta, si occupa di accendere/spegnere sui dispositivi connessi
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GO_Click(object sender, RoutedEventArgs e)
        {

            Analyzer result = await WebClientLuis.Order(CommandTextBox.Text);

            IEnumerable<GpioPin> pins = SelectedPin(result.Entity);
            foreach (GpioPin pin in pins)
            {
                switch (result.Intent)
                {
                    case Intent.TurnOn:
                        pin.Write(GpioPinValue.Low);
                        break;
                    case Intent.TurnOff:
                        pin.Write(GpioPinValue.High);
                        break;
                    case Intent.None:
                        break;
                }
            }

            CommandTextBox.Text = "";
            CommandTextBox.Focus(FocusState.Keyboard);
        }

        /// <summary>
        /// Termina l'esecuzione dell'applicazione
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Exit_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }
    }
}
