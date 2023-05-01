using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace GuideMe.Services
{
    public class BluetoothService
    {
        /// <summary>
        /// Obtém as permissões necessárias para escanear e conectar com dispositivos Bluetooths.
        /// As duas permissões necessárias são: ACCESS_COARSE_LOCATION e ACCESS_FINE_LOCATION.
        /// Esse método já busca conseguir essas permissões durante o uso do app.
        /// </summary>
        /// <returns></returns>
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
    }
}
