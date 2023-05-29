using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Bluetooth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using GuideMe.Interfaces;
using Xamarin.Forms;
using GuideMe.Droid;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Plugin.BLE;
using Android.Content.PM;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions;
using System.Collections.ObjectModel;

[assembly: Dependency(typeof(AndroidBluetoothService))]
namespace GuideMe.Droid
{
    public class AndroidBluetoothService : IAndroidBluetoothService
    {
        private readonly Plugin.BLE.Abstractions.Contracts.IAdapter _bluetoothAdapter;
        private readonly List<IDevice> _dispositivosEscaneados = new List<IDevice>();
        IDevice _device;

        public AndroidBluetoothService()
        {
            _bluetoothAdapter = CrossBluetoothLE.Current.Adapter;
            _bluetoothAdapter.DeviceDiscovered += (sender, dispositivoEncontrado) =>
            {
                if (dispositivoEncontrado.Device != null && !string.IsNullOrEmpty(dispositivoEncontrado.Device.Name))
                    _dispositivosEscaneados.Add(dispositivoEncontrado.Device);
            };
        }

        public void AbreTelaConfiguracoes()
        {
            Intent intent = new Intent(Android.Provider.Settings.ActionBluetoothSettings);
            intent.AddFlags(ActivityFlags.NewTask);
            Android.App.Application.Context.StartActivity(intent);
        }

        /// <summary>
        /// Permissao necessaria para dispositivos Android 11 ou mais antigos.
        /// </summary>
        /// <returns></returns>
        public async Task<PermissionStatus> ObtemPermissaoLocalizacao()
        {
            PermissionStatus permissionStatus = PermissionStatus.Unknown;
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                permissionStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (permissionStatus != PermissionStatus.Granted)
                {
                    permissionStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    if (permissionStatus != PermissionStatus.Granted)
                    {
                        permissionStatus = PermissionStatus.Denied; // Permission denied
                    }
                    else
                    {
                        permissionStatus = PermissionStatus.Granted;
                    }
                }
            }
            return permissionStatus;
        }

        /// <summary>
        /// Permissao necessaria para dispositvos Android 12 ou mais novos.
        /// </summary>
        /// <returns></returns>
        public async Task<PermissionStatus> ObtemPermissaoBluetoothLE()
        {
            PermissionStatus permissionStatus = PermissionStatus.Unknown;
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                permissionStatus = await Permissions.CheckStatusAsync<BLEPermission>();
                if (permissionStatus != PermissionStatus.Granted)
                {
                    permissionStatus = await Permissions.RequestAsync<BLEPermission>();
                    if (permissionStatus != PermissionStatus.Granted)
                    {
                        permissionStatus = PermissionStatus.Denied; // Permission denied
                    }
                    else
                    {
                        permissionStatus = PermissionStatus.Granted;
                    }
                }
            }
            return permissionStatus;
        }

        public async void EscanearDispositivosEConectarAoESP32()
        {
            List<IDevice> scannedDevices = await EscanearDispositivosAsync();
            _device = _dispositivosEscaneados.FirstOrDefault(d => d.Name == "ESP32-BLE-Server");

            if (_device != null)
                await ConectarAoESP32(_device);

            else
            {
                // Handle the case when the desired device is not found
                // Display an error message or take appropriate action
            }
        }

        public async Task<List<IDevice>> EscanearDispositivosAsync()
        {
            try
            {
                _dispositivosEscaneados.Clear();
                List<IDevice> dispositivos = new List<IDevice>();

                if (!_bluetoothAdapter.IsScanning)
                {                    
                    Task scanningTask = _bluetoothAdapter.StartScanningForDevicesAsync();
                    await scanningTask;
                    await _bluetoothAdapter.StopScanningForDevicesAsync();
                    
                    foreach (IDevice dispositivo in _dispositivosEscaneados)
                    {
                        dispositivos.Add(dispositivo);
                    }
                }

                return dispositivos;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task ConectarAoESP32(IDevice device)
        {
            try
            {
                //await _adapter.StopScanningForDevicesAsync(); // Stop scanning before connecting to a device

                // Connect to the device
                if (device != null)
                {
                    if (device.State == DeviceState.Connected)
                        return;

                    else
                    {
                        var parametrosDeConexao = new ConnectParameters(false, true);
                        await _bluetoothAdapter.ConnectToDeviceAsync(device, parametrosDeConexao);
                        var connectedDevice = _bluetoothAdapter.ConnectedDevices.FirstOrDefault();
                        //var connectedDevice = _bluetoothAdapter.ConnectedDevices.Where(d => d.Name == "ESP32-BLE-Server"); <-- alternativa
                        var service = await connectedDevice.GetServiceAsync(Guid.Parse("4FAFC201-1FB5-459E-8FCC-C5C9C331914B"));
                        var characteristic = await service.GetCharacteristicAsync(Guid.Parse("BEB5483E-36E1-4688-B7F5-EA07361B26A8"));
                        var bytes = await characteristic.ReadAsync();

                        string valorLidoCaracteristica = bytes.ToString();


                        // Perform further operations with the connected device
                        // For example, you can discover services and characteristics, read/write values, etc.

                        // After you finish working with the device, you can disconnect it
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public bool VerificaSeOBluetoothEstaAtivado()
        {
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
            return adapter?.IsEnabled ?? false;
        }

        public bool BluetoothLEEhSuportado()
        {
            PackageManager packageManager = Android.App.Application.Context.PackageManager;
            return packageManager.HasSystemFeature(PackageManager.FeatureBluetoothLe);
        }

        public string ObterVersaoDoAndroid()
        {
            return Build.VERSION.Release;
        }
    }
}