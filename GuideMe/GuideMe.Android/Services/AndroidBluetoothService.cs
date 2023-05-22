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

        public bool VerificaSeOBluetoothEstaAtivado()
        {
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
            return adapter?.IsEnabled ?? false;
        }
    }
}