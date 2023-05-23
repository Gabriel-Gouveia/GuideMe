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
        private IAndroidBluetoothService _bluetoothService;
        IDevice _device;

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
            _bluetoothService = DependencyService.Get<IAndroidBluetoothService>();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_bluetoothService.BluetoothLEEhSuportado())
            {
                PermissaoBLE = await _bluetoothService.ObtemPermissao();

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
                        {
                            _bluetoothService.AbreTelaConfiguracoes();
                            //oBluetoothTaAtivado = _bluetoothService.VerificaSeOBluetoothEstaAtivado();
                        }
                    }

                    else
                    {
                        try
                        {
                            await Task.Run(PerformaOperacoesBluetooth);
                        }

                        catch (Exception ex)
                        {
                            await DisplayAlert("Exceção", "Deu a exceção do Java Security pedindo o bluetooth connection.", "Ok");
                        }
                    }
                }
            }

            else
            {
                await DisplayAlert("Bluetooth LE não suportado", "O seu dispositivo não possui suporte ao BluetoothLE.", "Ok");
                return;
            }

            // Perform other tasks after the permission is granted
        }

        private async Task PerformaOperacoesBluetooth()
        {
            List<IDevice> dispositivosEscaneados = await _bluetoothService.EscanearDispositivos();
            _device = dispositivosEscaneados.FirstOrDefault(d => d.Name == "ESP32-BLE-Server");

            if (_device != null)
                await _bluetoothService.ConectarAoESP32(_device);
            else
            {
                // Handle the case when the desired device is not found
                // Display an error message or take appropriate action
            }
        }

    }
}
