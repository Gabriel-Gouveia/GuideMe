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
        private IDevice _dispositivoConectado;
        private IService _servicoUtilizado;
        private List<ICharacteristic> _caracteristicas = new List<ICharacteristic>();
        private ICharacteristic _caracteristicaUsada;

        private string _versaoDoAndroid;

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
            _bluetoothService = DependencyService.Get<IAndroidBluetoothService>();
            _versaoDoAndroid = _bluetoothService.ObterVersaoDoAndroid();
            _caracteristicaUsada = null;
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
                EscanearEConectarAoESP32();
            }
        }

        private async void EscanearEConectarAoESP32()
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
                    _dispositivoConectado = await _bluetoothService.EscanearDispositivosEConectarAoESP32Async();
                    
                    if (_dispositivoConectado == null)
                    {
                        await DisplayAlert("Erro ao conectar ao ESP32", "Tente conectar ao ESP32 novamente.", "Ok");
                        return;
                    }

                    _servicoUtilizado = await _bluetoothService.ObterServicoUtilizado(Guid.Parse("4FAFC201-1FB5-459E-8FCC-C5C9C331914B"), _dispositivoConectado);
                    _caracteristicaUsada = await _bluetoothService.ObterCaracteristicaUtilizada(Guid.Parse("BEB5483E-36E1-4688-B7F5-EA07361B26A8"), _servicoUtilizado);
                    _caracteristicas = await _bluetoothService.ObterListaDeCaracteristicas(_servicoUtilizado);
                    ConfiguraOEventoValueUpdatedDaCaracteristica();

                    //else
                    //{
                        //byte[] dadoRFID = await _bluetoothService.LeDadosRFIDAsync(_dispositivoConectado);
                        //string abc = Encoding.UTF8.GetString(dadoRFID);
                    //}
                }

                catch (Exception ex)
                {
                    await DisplayAlert("Exceção", "Deu a exceção do Java Security pedindo o bluetooth connection.", "Ok");
                }
            }
        }

        private async void ConfiguraOEventoValueUpdatedDaCaracteristica()
        {
            _caracteristicaUsada.ValueUpdated += (o, args) =>
            {

            };

            await _caracteristicaUsada.StartUpdatesAsync();
        }
    }
}
