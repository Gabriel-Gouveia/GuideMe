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
using Plugin.BLE.Abstractions.Exceptions;

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
                        permissionStatus = PermissionStatus.Denied;

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
                        permissionStatus = PermissionStatus.Denied;

                    else
                    {
                        permissionStatus = PermissionStatus.Granted;
                    }
                }
            }
            return permissionStatus;
        }

        public async Task<IDevice> EscanearDispositivosEConectarAoESP32Async()
        {
            List<IDevice> scannedDevices = await EscanearDispositivosAsync();
            _device = _dispositivosEscaneados.FirstOrDefault(d => d.Name == "ESP32-BLE-Server");

            if (_device != null)
               return  await ConectarAoESP32Async(_device);

            else
            {
                // Handle the case when the desired device is not found
                // Display an error message or take appropriate action
                return null;
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
            catch (DeviceDiscoverException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IDevice> ConectarAoESP32Async(IDevice device)
        {
            try
            {
                //await _adapter.StopScanningForDevicesAsync(); // Stop scanning before connecting to a device

                // Connect to the device
                if (device != null)
                {
                    if (device.State == DeviceState.Connected)
                        return device;

                    else
                    {
                        var parametrosDeConexao = new ConnectParameters(false, true);
                        await _bluetoothAdapter.ConnectToDeviceAsync(device, parametrosDeConexao);
                        return _bluetoothAdapter.ConnectedDevices.FirstOrDefault();
                        //var connectedDevice = _bluetoothAdapter.ConnectedDevices.FirstOrDefault();
                        //var connectedDevice = _bluetoothAdapter.ConnectedDevices.Where(d => d.Name == "ESP32-BLE-Server"); <-- alternativa
                        //var service = await connectedDevice.GetServiceAsync(Guid.Parse("4FAFC201-1FB5-459E-8FCC-C5C9C331914B"));
                        //var characteristic = await service.GetCharacteristicAsync(Guid.Parse("BEB5483E-36E1-4688-B7F5-EA07361B26A8"));
                        //var bytes = await characteristic.ReadAsync();

                        //string valorLidoCaracteristica = bytes.ToString();


                        // Perform further operations with the connected device
                        // For example, you can discover services and characteristics, read/write values, etc.

                        // After you finish working with the device, you can disconnect it
                    }
                }
                return null;
            }
            catch (DeviceConnectionException ex)
            {
                throw ex;
            }
            catch (CharacteristicReadException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<IService> ObtemServicoBLEAsync(IDevice dispositivoConectado)
        {
            if (dispositivoConectado.Name == "ESP32-BLE-Server" && dispositivoConectado.State == DeviceState.Connected)
                return await _device.GetServiceAsync(Guid.Parse("4FAFC201-1FB5-459E-8FCC-C5C9C331914B"));

            return null;
        }

        private async Task<ICharacteristic> LeCaracteristicaAsync(IDevice dispositivoConectado)
        {
            IService service = await ObtemServicoBLEAsync(dispositivoConectado);

            if (service != null)
                return await service.GetCharacteristicAsync(Guid.Parse("BEB5483E-36E1-4688-B7F5-EA07361B26A8"));

            return null;
        }

        public async Task<byte[]> LeDadosRFIDAsync(IDevice dispositivoConectado)
        {
            try
            {
                ICharacteristic characteristic = await LeCaracteristicaAsync(dispositivoConectado);

                if (characteristic != null)
                {
                    var dados = await characteristic.ReadAsync();
                    return dados;
                }

                return null;
            }

            catch (Exception ex)
            {
                return null;
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

        public async void ReiniciarOAppAposFalha()
        {
            // Inside the method where you handle the "Ok" button tap
            if (await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Erro", "Ocorreu um erro desconhecido." +
                "\nPor favor, reinicie o app.", "Ok", "Cancel"))
            {
                // Restart the app
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    var packageManager = Android.App.Application.Context.PackageManager;
                    var intent = packageManager.GetLaunchIntentForPackage(Android.App.Application.Context.PackageName);
                    intent.AddFlags(ActivityFlags.ClearTop);
                    intent.AddFlags(ActivityFlags.NewTask);
                    Android.App.Application.Context.StartActivity(intent);
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                }
                else
                {
                    var intent = Android.App.Application.Context.PackageManager.GetLaunchIntentForPackage(Android.App.Application.Context.PackageName);
                    intent.AddFlags(ActivityFlags.ClearTop);
                    intent.AddFlags(ActivityFlags.NewTask);
                    Android.App.Application.Context.StartActivity(intent);
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                }
            }
        }

        public async Task<IService> ObterServicoUtilizado(Guid UuidServico, IDevice dispositivoConectado)
        {
            if (dispositivoConectado != null && dispositivoConectado.State == DeviceState.Connected)
                return await dispositivoConectado.GetServiceAsync(UuidServico);

            return null;
        }

        public async Task<ICharacteristic> ObterCaracteristicaUtilizada(Guid UuidCaracteristica, IService servicoUtilizado)
        {
            if (servicoUtilizado != null)
                return await servicoUtilizado.GetCharacteristicAsync(UuidCaracteristica);

            return null;
        }

        public async Task<List<ICharacteristic>> ObterListaDeCaracteristicas(IService servicoUtilizado)
        {
            if (servicoUtilizado != null)
            {
                var listaCaracteristicasReadOnly = await servicoUtilizado.GetCharacteristicsAsync();

                List<ICharacteristic> listaCaracteristicas =  new List<ICharacteristic>();

                foreach (var caracteristica in listaCaracteristicasReadOnly)
                {
                    listaCaracteristicas.Add(caracteristica);
                }
            }

            return null;
        }
    }
}