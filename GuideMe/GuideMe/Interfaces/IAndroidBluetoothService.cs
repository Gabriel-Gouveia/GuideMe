using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace GuideMe.Interfaces
{
    public interface IAndroidBluetoothService
    {
        bool VerificaSeOBluetoothEstaAtivado();
        Task<PermissionStatus> ObtemPermissao();
        void AbreTelaConfiguracoes();
        void EscanearOESP32();
    }
}
