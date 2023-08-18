using Xamarin.Forms;
using Xamarin.Essentials;
using GuideMe.Interfaces;
using Plugin.BLE.Abstractions.Contracts;
using GuideMe.DAO;
using System;
using System.Threading.Tasks;
using System.Threading;
using Xamarin.CommunityToolkit.Extensions;
using System.Collections.Generic;

namespace GuideMe
{
    public partial class MainPage : ContentPage
    {
        private bool varTeste = false;
        public PermissionStatus PermissaoBLE { get; set; } = PermissionStatus.Unknown;
        public PermissionStatus PermissaoBLEAndroid12 { get; set; } = PermissionStatus.Unknown;
        private IAndroidBluetoothService _bluetoothService;
        IDevice _device;
        private string _versaoDoAndroid;

        private bool _threadLeituraTag = false;

        public MainPage()
        {
            InitializeComponent();          
            BindingContext = this;
            _bluetoothService = DependencyService.Get<IAndroidBluetoothService>();
            _versaoDoAndroid = _bluetoothService.ObterVersaoDoAndroid();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            VerificaCondicoesBluetooth();
        }

        private async void VerificaCondicoesBluetooth()
        {
            try
            {
                if (_bluetoothService.BluetoothLEEhSuportado())
                {
                    if (Convert.ToInt32(_versaoDoAndroid) >= 12)
                    {
                        bool permissaoBLEAndroid12Concedida = await ObterPermissaoBluetoothLEAndroid12Async();

                        if (!permissaoBLEAndroid12Concedida)
                            return;
                    }

                    ObterPermissaoLocalizacaoParaBluetoothLE();
                }

                else
                {
                    await DisplayAlert("Bluetooth LE não suportado", "O seu dispositivo não possui suporte ao BluetoothLE.", "Ok");
                    return;
                }
            }
            catch (Plugin.BLE.Abstractions.Exceptions.DeviceDiscoverException ex)
            {
                // TALKBACK
                await DisplayAlert("Falha ao escanear por dispositivos Bluetooth", "Ocorreu um erro ao escanear dispositivos Bluetooth.", "Ok");
                return;
            }
            catch (Plugin.BLE.Abstractions.Exceptions.DeviceConnectionException ex)
            {
                // TALKBACK
                await DisplayAlert("O ESP32 não foi conectado", "Ocorreu uma falha para conectar ao ESP32 por Bluetooth.", "Ok");
                return;
            }
            catch (Plugin.BLE.Abstractions.Exceptions.CharacteristicReadException ex)
            {
                // TALKBACK
                await DisplayAlert("Falha ao obter dados do leitor RFID", "Não foi possível ler dados retornados pelo ESP32.", "Ok");
                return;
            }
            catch (Exception ex)
            {
                // TALKBACK
                _bluetoothService.ReiniciarOAppAposFalha();
                return;
            }
        }

        private async Task<bool> ObterPermissaoBluetoothLEAndroid12Async()
        {
            PermissaoBLEAndroid12 = await _bluetoothService.ObtemPermissaoBluetoothLE();

            if (PermissaoBLEAndroid12 == PermissionStatus.Denied || PermissaoBLEAndroid12 == PermissionStatus.Disabled)
            {
                await DisplayAlert("Uso de Bluetooth não autorizado", "Não é possível usar o app sem o Bluetooth.", "Ok");
                return false;
            }
            return true;
        }

        private async void ObterPermissaoLocalizacaoParaBluetoothLE()
        {
            PermissaoBLE = await _bluetoothService.ObtemPermissaoLocalizacao();

            // Talkback depois
            if (PermissaoBLE == PermissionStatus.Denied || PermissaoBLE == PermissionStatus.Disabled)
                await DisplayAlert("Uso de Bluetooth não autorizado", "Não é possível usar o app sem o Bluetooth.", "Ok");

            else
            {
                bool oBluetoothTaAtivado = _bluetoothService.VerificaSeOBluetoothEstaAtivado();

                if (!oBluetoothTaAtivado)
                {
                    bool decisao = await DisplayAlert("O Bluetooth do dispositivo está desativado", "Para o GuideMe funcionar, é necessário que o Bluetooth esteja ativado." +
                        "\nDeseja ativar o Bluetooth?", "Yes", "No");

                    if (decisao)
                        _bluetoothService.AbreTelaConfiguracoes();
                }

                else if(!string.IsNullOrEmpty(StorageDAO.NomeBengalaBluetooth) && !_threadLeituraTag)
                {
                    try
                    {
                        _device = await /*Task.Run(*/_bluetoothService.EscanearDispositivosEConectarAoESP32Async(StorageDAO.NomeBengalaBluetooth);/*)*/
                        if (_device != null)
                        {
                            _threadLeituraTag = true;
                            await this.DisplayToastAsync("bengala conectada com sucesso!", 5000);
                            await DisplayAlert("Aviso", "bengala conectada com sucesso!", "Ok");
                            _ = Task.Factory.StartNew(_ => LeituraTagsBengala(), TaskCreationOptions.LongRunning);
                            
                        }
                        else
                            await DisplayAlert("Aviso", "Não foi possível conectar com a bengala", "Ok");

                    }

                    catch (Exception ex)
                    {
                        await DisplayAlert("Exceção", "Deu a exceção do Java Security pedindo o bluetooth connection.", "Ok");
                    }
                }
            }
        }
        public static string ConvertHex(string hexString)
        {
            try
            {
                string ascii = string.Empty;

                for (int i = 0; i < hexString.Length; i += 2)
                {
                    String hs = string.Empty;

                    hs = hexString.Substring(i, 2);
                    uint decval = System.Convert.ToUInt32(hs, 16);
                    char character = System.Convert.ToChar(decval);
                    ascii += character;

                }

                return ascii;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            return string.Empty;
        }
        private async void LeituraTagsBengala()
        {
            FrameLeituraTag frame = null;
            try
            {
                while (_threadLeituraTag && _device!=null)
                {
                    byte[] dadoRFID = await _bluetoothService.LeDadosRFIDAsync(_device);

                    if (dadoRFID != null)
                    {
                        string leitura = "";
                        foreach (byte b in dadoRFID)
                            leitura += ((char)b).ToString();

                        string[] tokens = leitura.Split(' ');
                        leitura = "";
                        foreach (string s in tokens)
                        {
                            string aux = s;

                            if (aux.Length == 1)
                                aux = "0" + s;

                            leitura += aux+" ";
                        }
                        //leitura = ConvertHex(leitura);

                        //leitura = System.Text.Encoding.ASCII.GetString(dadoRFID);

                        leitura = leitura.ToUpper().Trim();
                        var frameLido = ParserAntena.ParseData(leitura);
                        if (frameLido != null)
                        {
                            if (frameLido.TipoFrame == TrataFrames.LeituraTag)
                            {
                                if (frame == null || frame.TagID != (frameLido as FrameLeituraTag).TagID)
                                {
                                    frame = (frameLido as FrameLeituraTag);
                                    await this.DisplayToastAsync($"Tag lida: {frame.TagID} ", 2000);
                                }
                               
                               

                            }
                        }
                    }

                    

                   
                    Thread.Sleep(350);
                }
            }
            catch (Exception err)
            {
                _ = Task.Factory.StartNew(_ => LeituraTagsBengala(), TaskCreationOptions.LongRunning);
            }
        }
      

        private async void btn_escanearBluetooth_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DispositivosBluetooth(_bluetoothService));
        }
    }
}
