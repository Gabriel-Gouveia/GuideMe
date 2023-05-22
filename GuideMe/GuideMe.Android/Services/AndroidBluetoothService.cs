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

[assembly: Dependency(typeof(AndroidBluetoothService))]
namespace GuideMe.Droid
{
    public class AndroidBluetoothService : IAndroidBluetoothService
    {
        IBluetoothLE _ble = CrossBluetoothLE.Current;
        Plugin.BLE.Abstractions.Contracts.IAdapter _adapter = CrossBluetoothLE.Current.Adapter;
        public void AbreTelaConfiguracoes()
        {
            Intent intent = new Intent(Android.Provider.Settings.ActionBluetoothSettings);
            intent.AddFlags(ActivityFlags.NewTask);
            Android.App.Application.Context.StartActivity(intent);
        }

        public async Task<PermissionStatus> ObtemPermissao()
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

        public async void EscanearOESP32()
        {
            //try
            //{
            //    List<IDevice> deviceList = new List<IDevice>();

            //    // Start scanning for devices
            //    await _adapter.StartScanningForDevicesAsync();

            //    _adapter.DeviceDiscovered += (s, a) =>
            //    {
            //        // Add discovered devices to the list
            //        deviceList.Add(a.Device);

            //        // Check if the discovered device matches your criteria
            //        if (a.Device.Name == "ESP32-BLE-Server")
            //        {
            //            // Connect to the device
            //            ConectarAoESP32(a.Device);
            //        }
            //    };                
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
            try
            {
                List<IDevice> deviceList = new List<IDevice>();
                //_adapter.ScanMode = Plugin.BLE.Abstractions.Contracts.ScanMode.LowPower;
                //var scanFilterOptions = new ScanFilterOptions();
                //scanFilterOptions.ServiceUuids = new[] { new Guid("4FAFC201-1FB5-459E-8FCC-C5C9C331914B") }; // cross platform filter
                //scanFilterOptions.ManufacturerDataFilters = new[] { new ManufacturerDataFilter(1), new ManufacturerDataFilter(2) }; // android only filter
                //scanFilterOptions.DeviceAddresses = new[] { "80:6F:B0:43:8D:3B", "80:6F:B0:25:C3:15", etc }; // android only filter
                //await _adapter.StartScanningForDevicesAsync(scanFilterOptions);
                await _adapter.StartScanningForDevicesAsync();
               // _adapter.DeviceDiscovered += (s, a) => deviceList.Add(a.Device);
                deviceList = (List<IDevice>)_adapter.DiscoveredDevices;
                IDevice wishedDevice = deviceList.Where(d => d.Name == "ESP32-BLE-Server").First();
                await ConectarAoESP32(wishedDevice);
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
                await _adapter.StopScanningForDevicesAsync(); // Stop scanning before connecting to a device

                // Connect to the device
                await _adapter.ConnectToDeviceAsync(device);
                var connectedDevice = _adapter.ConnectedDevices.FirstOrDefault();
                var service = await connectedDevice.GetServiceAsync(Guid.Parse("4FAFC201-1FB5-459E-8FCC-C5C9C331914B"));
                var characteristic = await service.GetCharacteristicAsync(Guid.Parse("BEB5483E-36E1-4688-B7F5-EA07361B26A8"));
                var bytes =  await characteristic.ReadAsync();

                string valorLidoCaracteristica = bytes.ToString();

                // Perform further operations with the connected device
                // For example, you can discover services and characteristics, read/write values, etc.

                // After you finish working with the device, you can disconnect it
                
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







        //private async Task ConnectToDevice(IDevice device)
        //{
        //    // Connect to the device
        //    var connection = await device.ConnectAsync();

        //    if (connection.State == ConnectionState.Connected)
        //    {
        //        // Once connected, discover characteristics
        //        device.WhenAnyCharacteristicDiscovered().Subscribe(characteristic =>
        //        {
        //            // Perform desired operations with the characteristic
        //            // Read, write, or subscribe to notifications here
        //            // ...
        //        });
        //    }
        //    else if (connection.State == ConnectionState.Disconnected)
        //    {
        //        // Handle disconnection or any other relevant logic
        //    }
        //}


        //public async void EscanearOESP32()
        //{
        //    // Start scanning for devices
        //    CrossBleAdapter.Current.Scan().Subscribe(scanResult =>
        //    {
        //        // Check if the scanned device's name matches the target ESP32 name
        //        if (scanResult.Device.Name == "YourESP32Name")
        //        {
        //            // Stop scanning
        //            CrossBleAdapter.Current.StopScan();

        //            // Connect to the ESP32 device
        //            scanResult.Device.Connect().Subscribe(connectionState =>
        //            {
        //                if (connectionState == ConnectionState.Connected)
        //                {
        //                    // Once connected, discover characteristics
        //                    scanResult.Device.WhenAnyCharacteristicDiscovered().Subscribe(characteristic =>
        //                    {
        //                        // Perform desired operations with the characteristic
        //                        // Read, write, or subscribe to notifications here
        //                        // ...
        //                    });
        //                }
        //                else if (connectionState == ConnectionState.Disconnected)
        //                {
        //                    // Handle disconnection or any other relevant logic
        //                }
        //            });
        //        }
        //    });
        //}

        //public async void EscanearOESP32()
        //{
        //    // discover some devices
        //    CrossBleAdapter.Current.Scan().Subscribe(scanResult => {  });

        //    // Once finding the device/scanresult you want
        //    scanResult.Device.Connect();

        //    scanResult.Device.WhenAnyCharacteristicDiscovered().Subscribe(characteristic => {
        //        // read, write, or subscribe to notifications here
        //        var result = await characteristic.Read(); // use result.Data to see response
        //        await characteristic.Write(bytes);

        //        characteristic.EnableNotifications();
        //        characteristic.WhenNotificationReceived().Subscribe(result => {
        //            //result.Data to get at response
        //        });
        //    });
        //}
    }
}