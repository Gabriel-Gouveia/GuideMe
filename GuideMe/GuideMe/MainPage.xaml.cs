using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Essentials;
using GuideMe.Services;
using System.Diagnostics;
using GuideMe.Interfaces;
using Plugin.BLE.Abstractions.Contracts;

namespace GuideMe
{
    public partial class MainPage : ContentPage
    {
        public PermissionStatus PermissaoBLE { get; set; } = PermissionStatus.Unknown;
        public PermissionStatus PermissaoBLEAndroid12 { get; set; } = PermissionStatus.Unknown;
        private IAndroidBluetoothService _bluetoothService;
        IDevice _dispositivoConectado;
        private string _versaoDoAndroid;

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
            _bluetoothService = DependencyService.Get<IAndroidBluetoothService>();
            _versaoDoAndroid = _bluetoothService.ObterVersaoDoAndroid();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            VerificaCondicoesBluetooth();
        }

        private async void VerificaCondicoesBluetooth()
        {
            try
            {
                if (_bluetoothService.BluetoothLEEhSuportado())
                {
                    if (Convert.ToInt32(_versaoDoAndroid) >= 12)
                    {
                        bool permissaoBLEAndroid12Concedida = await ObterPermissaoBluetoothLEAndroid12Async();

                        if (!permissaoBLEAndroid12Concedida)
                            return;
                    }

                    ObterPermissaoLocalizacaoParaBluetoothLE();
                }

                else
                {
                    await DisplayAlert("Bluetooth LE não suportado", "O seu dispositivo não possui suporte ao BluetoothLE.", "Ok");
                    return;
                }
            }
            catch (Plugin.BLE.Abstractions.Exceptions.DeviceDiscoverException ex)
            {
                // TALKBACK
                await DisplayAlert("Falha ao escanear por dispositivos Bluetooth", "Ocorreu um erro ao escanear dispositivos Bluetooth.", "Ok");
                return;
            }
            catch (Plugin.BLE.Abstractions.Exceptions.DeviceConnectionException ex)
            {
                // TALKBACK
                await DisplayAlert("O ESP32 não foi conectado", "Ocorreu uma falha para conectar ao ESP32 por Bluetooth.", "Ok");
                return;
            }
            catch (Plugin.BLE.Abstractions.Exceptions.CharacteristicReadException ex)
            {
                // TALKBACK
                await DisplayAlert("Falha ao obter dados do leitor RFID", "Não foi possível ler dados retornados pelo ESP32.", "Ok");
                return;
            }
            catch (Exception ex)
            {
                // TALKBACK
                _bluetoothService.ReiniciarOAppAposFalha();
                return;
            }
        }

        private async Task<bool> ObterPermissaoBluetoothLEAndroid12Async()
        {
            PermissaoBLEAndroid12 = await _bluetoothService.ObtemPermissaoBluetoothLE();

            if (PermissaoBLEAndroid12 == PermissionStatus.Denied || PermissaoBLEAndroid12 == PermissionStatus.Disabled)
            {
                await DisplayAlert("Uso de Bluetooth não autorizado", "Não é possível usar o app sem o Bluetooth.", "Ok");
                return false;
            }
            return true;
        }

        private async void ObterPermissaoLocalizacaoParaBluetoothLE()
        {
            PermissaoBLE = await _bluetoothService.ObtemPermissaoLocalizacao();

            // Talkback depois
            if (PermissaoBLE == PermissionStatus.Denied || PermissaoBLE == PermissionStatus.Disabled)
                await DisplayAlert("Uso de Bluetooth não autorizado", "Não é possível usar o app sem o Bluetooth.", "Ok");

            else
            {
                bool oBluetoothTaAtivado = _bluetoothService.VerificaSeOBluetoothEstaAtivado();

                if (!oBluetoothTaAtivado)
                {
                    bool decisao = await DisplayAlert("O Bluetooth do dispositivo está desativado", "Para o GuideMe funcionar, é necessário que o Bluetooth esteja ativado." +
                        "\nDeseja ativar o Bluetooth?", "Yes", "No");

                    if (decisao)
                        _bluetoothService.AbreTelaConfiguracoes();
                }

                else
                {
                    try
                    {
                        _dispositivoConectado = await /*Task.Run(*/_bluetoothService.EscanearDispositivosEConectarAoESP32Async();/*)*/

                        if (_dispositivoConectado == null)
                        {

                        }

                        else
                        {
                            byte[] dadoRFID = await _bluetoothService.LeDadosRFIDAsync(_dispositivoConectado);
                            string abc = Encoding.UTF8.GetString(dadoRFID);
                        }
                    }

                    catch (Exception ex)
                    {
                        await DisplayAlert("Exceção", "Deu a exceção do Java Security pedindo o bluetooth connection.", "Ok");
                    }
                }
            }
        }
    }
}
