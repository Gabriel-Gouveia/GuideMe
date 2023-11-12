using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace GuideMe.Interfaces
{
    public delegate void OnBluetoothScanTerminado();
    public delegate void OnBluetoothDesconectado();
    public interface IAndroidBluetoothService
    {
        
        List<IDevice> _dispositivosEscaneados { get; set; }

        event OnBluetoothDesconectado OnDesconectado;

        event OnBluetoothScanTerminado OnBluetoothScanTerminado;
        bool VerificaSeOBluetoothEstaAtivado();
        Task<PermissionStatus> ObtemPermissaoLocalizacao();
        void AbreTelaConfiguracoes();
        Task<List<IDevice>> EscanearDispositivosAsync();
        bool BluetoothLEEhSuportado();
        Task<IDevice> EscanearDispositivosEConectarAoESP32Async(string espName);
        Task<IDevice> ConectarAoESP32Async(IDevice device);
        string ObterVersaoDoAndroid();
        Task<PermissionStatus> ObtemPermissaoBluetoothLE();
        void ReiniciarOAppAposFalha();
        Task<byte[]> LeDadosRFIDAsync(IDevice dispositivoConectado);
        Task<bool> ApagaUltimaTagLida(IDevice dispositivoConectado);
        Task<bool> AcionarVibracaoBengala(IDevice dispositivoConectado, int qtVibracoes);
    }
}
