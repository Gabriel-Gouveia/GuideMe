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

namespace GuideMe
{
    public partial class MainPage : ContentPage
    {
        public PermissionStatus PermissaoBLE { get; set; } = PermissionStatus.Unknown;
        private IAndroidBluetoothService _bluetoothService;
        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
            _bluetoothService = DependencyService.Get<IAndroidBluetoothService>();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
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
                    }
                }
            }

            // Perform other tasks after the permission is granted
        }
    }
}
