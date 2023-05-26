﻿using System;
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
        IDevice _device;
        private string _versaoDoAndroid;

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
            _bluetoothService = DependencyService.Get<IAndroidBluetoothService>();
            _versaoDoAndroid = _bluetoothService.ObterVersaoDoAndroid();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_bluetoothService.BluetoothLEEhSuportado())
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
                        {
                            _bluetoothService.AbreTelaConfiguracoes();
                            //oBluetoothTaAtivado = _bluetoothService.VerificaSeOBluetoothEstaAtivado();
                        }
                    }

                    else
                    {
                        try
                        {
                            await Task.Run(_bluetoothService.EscanearDispositivosEConectarAoESP32);
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
    }
}
