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
using GuideMe.DAO;

[assembly: Dependency(typeof(AndroidBluetoothService))]
namespace GuideMe.Droid
{
    public class AndroidBluetoothService : IAndroidBluetoothService
    {
        private readonly string ServicoBengala = "4fafc201-1fb5-459e-8fcc-c5c9c331914b";
        private readonly string MotorCharacteristc = "30305726-ec78-11ed-a05b-0242ac120003";
        private readonly string TAGCharacteristc = "BEB5483E-36E1-4688-B7F5-EA07361B26A8";
        private readonly Plugin.BLE.Abstractions.Contracts.IAdapter _bluetoothAdapter;
        public List<IDevice> dispositivosEscaneados = new List<IDevice>();
        bool lockCaracteristicaBluetooth = false;

        IDevice _device;

        List<IDevice> IAndroidBluetoothService._dispositivosEscaneados 
        { get => dispositivosEscaneados;
            set {
                dispositivosEscaneados = value;
            }
        }

        public event OnBluetoothScanTerminado OnBluetoothScanTerminado;

        public AndroidBluetoothService()
        {
            _bluetoothAdapter = CrossBluetoothLE.Current.Adapter;
            _bluetoothAdapter.DeviceDiscovered += async (sender, dispositivoEncontrado) =>
            {
                if (dispositivoEncontrado.Device != null && !string.IsNullOrEmpty(dispositivoEncontrado.Device.Name) && VerificaSeBengala(dispositivoEncontrado.Device))
                {
                    //var teste = await dispositivoEncontrado.Device.GetServicesAsync();
                    //var servico = await dispositivoEncontrado.Device.GetServiceAsync(Guid.Parse(ServicoBengala));
                    dispositivosEscaneados.Add(dispositivoEncontrado.Device);
                    if (Debugger.IsAttached)
                        Console.WriteLine($"Dispositivo encontrado! {dispositivoEncontrado.Device.Name}");
                }
            };
        }

        private bool VerificaSeBengala(IDevice device)
        {
            bool retorno = false;
            bool shortName = false;
            bool manufacturerData = false;
            try
            {
                var dados = device.AdvertisementRecords;
                foreach (var record in dados)
                {
                    string utfString = Encoding.ASCII.GetString(record.Data, 0, record.Data.Length);

                    if (record.Type == AdvertisementRecordType.ShortLocalName && utfString.Trim() == "BengTCC")
                        shortName = true;
                    else if (record.Type == AdvertisementRecordType.ManufacturerSpecificData && utfString.Trim() == "Rodrigo")
                        manufacturerData = true;
                }
                return shortName && manufacturerData;
            }
            catch
            {
                
            }



            return retorno;
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

        public async Task<IDevice> EscanearDispositivosEConectarAoESP32Async(string espName)
        {
            List<IDevice> scannedDevices = await EscanearDispositivosAsync();
            _device = dispositivosEscaneados.FirstOrDefault(d => d.Name == espName);

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

                dispositivosEscaneados.Clear();
                List<IDevice> dispositivos = new List<IDevice>();

                if (!_bluetoothAdapter.IsScanning)
                {
                    Task scanningTask = _bluetoothAdapter.StartScanningForDevicesAsync();
                    await scanningTask;
                    await _bluetoothAdapter.StopScanningForDevicesAsync();

                    foreach (IDevice dispositivo in dispositivosEscaneados)
                    {
                        dispositivos.Add(dispositivo);
                    }
                }


                OnBluetoothScanTerminado?.Invoke();

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
            if (dispositivoConectado.Name == StorageDAO.NomeBengalaBluetooth && dispositivoConectado.State == DeviceState.Connected)
                return await dispositivoConectado.GetServiceAsync(Guid.Parse(ServicoBengala));

            return null;
        }

        private async Task<ICharacteristic> LeCaracteristicaAsync(IDevice dispositivoConectado)
        {
            IService service = await ObtemServicoBLEAsync(dispositivoConectado);

            if (service != null)
                return await service.GetCharacteristicAsync(Guid.Parse(TAGCharacteristc));

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

        public async Task<bool> AcionarVibracaoBengala(IDevice dispositivoConectado, int qtVibracoes)
        {
            string comando = $"|{qtVibracoes.ToString()}";
            if (dispositivoConectado != null && dispositivoConectado.State == DeviceState.Connected && (dispositivoConectado.Name == StorageDAO.NomeBengalaBluetooth))
            {
                IService servicoBengala = await dispositivoConectado.GetServiceAsync(Guid.Parse(ServicoBengala));
                if (servicoBengala != null)
                {
                    ICharacteristic caracteristicaMotor = await servicoBengala.GetCharacteristicAsync(new Guid(MotorCharacteristc));
                    if (caracteristicaMotor != null)
                        return await caracteristicaMotor.WriteAsync(Encoding.ASCII.GetBytes(comando));
                    else
                        return false;
                }
                else
                    return false;

            }
            else
                return false;
        }
    }
}