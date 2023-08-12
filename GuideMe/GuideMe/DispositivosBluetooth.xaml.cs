using GuideMe.DAO;
using GuideMe.Interfaces;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace GuideMe
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DispositivosBluetooth : ContentPage
    {
        ObservableCollection<string> _devicesNames = new ObservableCollection<string>();
        public IAndroidBluetoothService BluetoothService { get; set; }
        public DispositivosBluetooth(IAndroidBluetoothService _bluetoothService)
        {
            InitializeComponent();
            BluetoothService = _bluetoothService;
            _bluetoothService.OnBluetoothScanTerminado -= _bluetoothService_OnBluetoothScanTerminado;
            _bluetoothService.OnBluetoothScanTerminado += _bluetoothService_OnBluetoothScanTerminado;
            _ =  _bluetoothService.EscanearDispositivosAsync();


            DisplayAlert("Aviso", "procurando dispositivos!", "Ok");



        }

        private void _bluetoothService_OnBluetoothScanTerminado()
        {
            BluetoothService.OnBluetoothScanTerminado -= _bluetoothService_OnBluetoothScanTerminado;
            List<IDevice> dispositivos= new List<IDevice>(BluetoothService._dispositivosEscaneados);
            if (dispositivos != null && dispositivos.Count > 0)
            {
                List<string>Nomes = new List<string>();
                foreach (IDevice device in dispositivos)
                {
                    if (!string.IsNullOrEmpty(device.Name))
                        Nomes.Add(device.Name);
                }
                _devicesNames = new ObservableCollection<string>(Nomes);
                listDevices.ItemsSource = _devicesNames;
                listDevices.IsVisible = true;
            }

        }

        private async void listDevices_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(listDevices.SelectedItem.ToString()))
            {
                if (await StorageDAO.SalvaConfiguracoesNomeBengala(listDevices.SelectedItem.ToString()))
                {
                    await DisplayAlert("Aviso", "bengala salva com sucesso!", "Ok");
                    await Navigation.PopAsync();
                }

                else
                    await DisplayAlert("Erro", "erro ao salvar a bengala", "Ok");
            }
        }
    }
}