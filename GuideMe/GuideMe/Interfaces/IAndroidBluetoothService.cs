using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
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
        Task<PermissionStatus> ObtemPermissaoLocalizacao();
        void AbreTelaConfiguracoes();
        Task<List<IDevice>> EscanearDispositivos();
        bool BluetoothLEEhSuportado();
        void EscanearDispositivosEConectarAoESP32();
        Task ConectarAoESP32(IDevice device);
        string ObterVersaoDoAndroid();
    }
}
