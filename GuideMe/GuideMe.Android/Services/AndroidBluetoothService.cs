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
using Plugin.BluetoothLE;

[assembly: Dependency(typeof(AndroidBluetoothService))]
namespace GuideMe.Droid
{
    public class AndroidBluetoothService : IAndroidBluetoothService
    {
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
            try
            {
                // Start scanning for devices
                CrossBleAdapter.Current.Scan().Subscribe(scanResult =>
                {
                // Check if the scanned device's name matches the target ESP32 name
                if (scanResult.Device.Name == "ESP32-BLE-Server")
                    {
                    // Stop scanning
                    CrossBleAdapter.Current.StopScan();

                    // Connect to the ESP32 device
                    //ConnectToDevice(scanResult.Device);

                    scanResult.Device.Connect();

                        scanResult.Device.WhenAnyCharacteristicDiscovered().Subscribe(characteristic =>
                        {
                        // read, write, or subscribe to notifications here

                        if (characteristic.Uuid.ToString() == "BEB5483E-36E1-4688-B7F5-EA07361B26A8")
                            {

                                var result = characteristic.Read(); // use result.Data to see response

                        }
                            characteristic.EnableNotifications();
                            characteristic.WhenNotificationReceived().Subscribe(result =>
                            {
                            //result.Data to get at response
                        });
                        });

                    }
                });
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