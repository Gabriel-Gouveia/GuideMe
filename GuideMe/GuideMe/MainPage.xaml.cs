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
using System.Text;
using GuideMe.Bengala;
using System.Collections.Concurrent;
using GuideMe.Gestos;
using System.Linq;
using GuideMe.Enum;
using Microsoft.CognitiveServices.Speech;
using GuideMe.STT;
using static GuideMe.STT.STTHelper;
using System.Diagnostics;
using GuideMe.Utils;
using GuideMe.Navegacao;

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
        private ConcurrentBag<RequisicaoBase> FilaRequsicoesBengala = new ConcurrentBag<RequisicaoBase>();
        private ConcurrentQueue<GestosBase> FilaGestos = new ConcurrentQueue<GestosBase>();
        private SpeechOptions _configuracoesFalaLocal = null;

        private List<string> lugaresMock = new List<string>();
        private bool _threadMensagensBengala = false;
        NavegacaoController navegacao = new NavegacaoController();

        public MainPage()
        {
            InitializeComponent();          
            BindingContext = this;
            _bluetoothService = DependencyService.Get<IAndroidBluetoothService>();
            STTHelper.InitService();
            STTHelper.OnComandoVozDetectado += ComandoVozDetectado;
            /*_versaoDoAndroid = _bluetoothService.ObterVersaoDoAndroid();
            lugaresMock.Add("Lojas Americanas");
            lugaresMock.Add("Banheiro");
            lugaresMock.Add("Havana");
            lugaresMock.Add("espoleto");
            lugaresMock.Add("chiquinho");
            lugaresMock.Add("mequi");
            lugaresMock.Add("burguer king");
            STTHelper.RegistrarLugares(lugaresMock);*/
            _ = Task.Factory.StartNew(_ => ControlePaginaPrincipal(), TaskCreationOptions.LongRunning);
            _ = Task.Factory.StartNew(_ => ControleGestos(), TaskCreationOptions.LongRunning);        
        }

        private async void ControlePaginaPrincipal()
        {
            string[,] dicas = new string[,] 
            { { "gifDireita.gif","Deslize 2 vezes para a direita para testar o dispositivo!"},
              { "gifCima.gif","Deslize 3 vezes para cima para iniciar um comando de voz!"},
              { "gifBaixo.gif","Deslize 3 vezes para baixo para procurar e se conectar com outro dispositivo!"}};
            int pos = 0;
            Stopwatch sw = new Stopwatch();
            try
            {
                bool forcaRecarregar = false;
                bool estavaGravando = false;
                while (true)
                {
                    if (!STTHelper.IsTranscribbing && (!sw.IsRunning || sw.ElapsedMilliseconds >= 8000 || forcaRecarregar))
                    {
                        estavaGravando = false;
                        forcaRecarregar = false;
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            try
                            {
                                lbDica.IsVisible = true;
                                lbDica.Text = dicas[pos, 1];
                                imagem.Source = dicas[pos, 0];
                                imagem.IsAnimationPlaying = true;
                                sw.Restart();
                            }
                            catch (Exception e)
                            { 
                            }
                        });

                        if (pos + 1 <3)
                            pos++;
                        else
                            pos = 0;
                    }
                    if (!STTHelper.IsTranscribbing && estavaGravando)
                        forcaRecarregar = true;

                    if (STTHelper.IsTranscribbing && !estavaGravando)
                    {
                        estavaGravando = true;
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            try
                            {
                                lbDica.IsVisible = false;
                                imagem.Source = "gifOuvindo.gif";
                                imagem.IsAnimationPlaying = true;
                            }
                            catch (Exception e)
                            {
                            }
                        });
                    }
                    

                    Thread.Sleep(350);
                }
            }
            catch (Exception err)
            {
                _ = Task.Factory.StartNew(_ => ControlePaginaPrincipal(), TaskCreationOptions.LongRunning);
            }
        }

        async void ComandoVozDetectado(object sender, ComandoVozEventArgs args)
        {
            if (args.Comando == EnumComandoVoz.Irpara)
            {
                //_ = TTSHelper.Speak($"Comando de voz detectado: {args.Comando.ToString()} {args.Lugar}");
                await TTSHelper.Speak($"Ok!. Calculando rota para: {args.Lugar}");
                navegacao.CalcularRota(args.Lugar);
            }
               
            else if (args.Comando != EnumComandoVoz.ListarLugares)
            {
                _ = TTSHelper.Speak($"Comando de voz detectado: {args.Comando.ToString()}");
            }
            else
            {
                await TTSHelper.Speak($"Comando de voz detectado: {args.Comando.ToString()}");
                foreach (var lugar in navegacao.All_Lugares_Navegaveis)
                    await TTSHelper.Speak(lugar.Nome);
            }
        }

        void UpdateTranscription(string newText)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (!string.IsNullOrWhiteSpace(newText))
                {
                    Console.WriteLine(newText);
                }
            });
        }

        void InsertDateTimeRecord()
        {
            var msg = $"=================\n{DateTime.Now.ToString()}\n=================";
            UpdateTranscription(msg);
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = TTSHelper.Speak("Bem vindo ao Guide-me !");
            if (_device==null)
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

                else if (!string.IsNullOrEmpty(StorageDAO.NomeBengalaBluetooth) && !_threadMensagensBengala)
                    ConectarNaBengala();
                else
                {
                    _ = TTSHelper.Speak("Nenhum dispositivo foi encontrado!");
                    ProcurarDispositivo();
                }
                    
            }
        }

        private void InicializaControleBengala()
        {
            _ = Task.Factory.StartNew(_ => MensagensBengala(), TaskCreationOptions.LongRunning);
            _ = Task.Factory.StartNew(_ => RequisitaLeiturasTags(), TaskCreationOptions.LongRunning);
            
            
        }
        private async void ConectarNaBengala()
        {
            try
            {


                _ = TTSHelper.Speak("Procurando dispositivos!");
                _device = await /*Task.Run(*/_bluetoothService.EscanearDispositivosEConectarAoESP32Async(StorageDAO.NomeBengalaBluetooth);/*)*/
                if (_device != null)
                {
                    _threadMensagensBengala = true;
                    bool apagouMsg = await _bluetoothService.ApagaUltimaTagLida(_device);
                    await _bluetoothService.AcionarVibracaoBengala(_device, 2);
                    _ = TTSHelper.Speak("Dispositivo conectado com sucesso!");
                    InicializaControleBengala();

                }
                else
                    _ = TTSHelper.Speak("Não foi possível se conectar com o dispositivo");

            }

            catch (Exception ex)
            {
                await DisplayAlert("Exceção", "Deu a exceção do Java Security pedindo o bluetooth connection.", "Ok");
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
        private void LimpaListaGestos(ref List<GestosBase> lista, bool forcarLimpeza=false)
        {
            List<GestosBase> novaLista = new List<GestosBase>();
            if (forcarLimpeza)
            {
                lista = novaLista;
                return;
            }
            foreach (GestosBase gesto in lista)
            {
                if (DateTime.Now.Subtract(gesto.Instantes).TotalSeconds < 2)
                    novaLista.Add(gesto);
            }
           lista = novaLista;
        }
        private EnumTipoAcaoGesto InterpretaGesto(string comandos)
        {
            if (string.IsNullOrEmpty(comandos))
                return EnumTipoAcaoGesto.None;

            switch (comandos)
            {
                case "swdswd":
                    return EnumTipoAcaoGesto.VibrarMotor;
                case "swbswbswb":
                    return EnumTipoAcaoGesto.ProcurarDispositivos;
                case "swcswcswc":
                    return EnumTipoAcaoGesto.ComandoVoz;
                default:
                    return EnumTipoAcaoGesto.None;

            }
        }

        private void ProcessaGesto(EnumTipoAcaoGesto comando)
        {
            Console.WriteLine($"Processando comando: {comando}");
            switch (comando)
            {
                case EnumTipoAcaoGesto.VibrarMotor:
                if (_device != null)
                    FilaRequsicoesBengala.Add(new RequisicaoMotor() { Tipo = Enum.EnumTipoRequisicaoBengala.AcionarMotor, QtVibracoes = 2 });
                    break;
                case EnumTipoAcaoGesto.ProcurarDispositivos:
                    _device?.Dispose();
                    _device = null;
                    _threadMensagensBengala = false;
                    VerificaCondicoesBluetooth();
                    break;
                case EnumTipoAcaoGesto.ComandoVoz:
                    STTHelper.StartListening();
                    break;
                default:
                    return ;

            }
        }
        private async void ControleGestos()
        {
            FrameLeituraTag ultimoFrameLido = null;
            List<GestosBase> gestosProcessados =  new List<GestosBase>();
            try
            {
                while (true)
                {
                    LimpaListaGestos(ref gestosProcessados);
                    GestosBase _gestoNovo = null;

                    if(FilaGestos.Count>0)
                    FilaGestos.TryDequeue(out _gestoNovo);

                    if (_gestoNovo!=null)
                        gestosProcessados.Add(_gestoNovo);


                    string comando = "";
                    foreach (GestosBase gesto in gestosProcessados)
                        comando += gesto.GetInfo();

                    if (!string.IsNullOrEmpty(comando))
                        Console.WriteLine($"Comando identificado: {comando}");

                    EnumTipoAcaoGesto tipoAcaoGesto = InterpretaGesto(comando);

                    if (tipoAcaoGesto != EnumTipoAcaoGesto.None)
                    {
                        ProcessaGesto(tipoAcaoGesto);
                        LimpaListaGestos(ref gestosProcessados,true);
                    }
                        

                    Thread.Sleep(350);
                }
            }
            catch (Exception err)
            {
                _ = Task.Factory.StartNew(_ => ControleGestos(), TaskCreationOptions.LongRunning);
            }
        }
        private async void MensagensBengala()
        {
            FrameLeituraTag ultimoFrameLido = null;
            try
            {
                while (_threadMensagensBengala && _device != null)
                {
                    if (FilaRequsicoesBengala != null && FilaRequsicoesBengala.Count > 0)
                    {
                        RequisicaoBase requisicao = null;
                        if (FilaRequsicoesBengala.TryTake(out requisicao))
                        {
                            switch (requisicao.Tipo)
                            {
                                case Enum.EnumTipoRequisicaoBengala.LeituraTag:
                                    FrameLeituraTag frameAux = await LeituraTagsBengala(ultimoFrameLido == null ? null : ultimoFrameLido.IDMensagem);
                                    if (frameAux != null)
                                        ultimoFrameLido = frameAux;
                                    break;
                                case Enum.EnumTipoRequisicaoBengala.AcionarMotor:
                                    if (requisicao is RequisicaoMotor)
                                    {
                                        await TTSHelper.Speak("Testando dispositivo");
                                        if (!await VibrarMotor(2))
                                            _ = this.DisplayToastAsync("Comando erro", 800);
                                    }
                                            
                                    break;
                            }
                        }

                        
                    }
                    Thread.Sleep(350);
                }
            }
            catch (Exception err)
            {
                _ = Task.Factory.StartNew(_ => MensagensBengala(), TaskCreationOptions.LongRunning);
            }
        }
        private  void RequisitaLeiturasTags()
        {
            try
            {
                while (_threadMensagensBengala && _device != null)
                {
                    FilaRequsicoesBengala.Add(new RequisicaoBase() { Tipo = Enum.EnumTipoRequisicaoBengala.LeituraTag });
                    Thread.Sleep(350);
                }
            }
            catch (Exception err)
            {
                _ = Task.Factory.StartNew(_ => RequisitaLeiturasTags(), TaskCreationOptions.LongRunning);
            }
        }
        private async Task<FrameLeituraTag> LeituraTagsBengala(string instanteMillis)
        {
            FrameLeituraTag frame = null;
            try
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

                        leitura += aux + " ";
                    }

                    leitura = leitura.ToUpper().Trim();
                    string[] tokensFinais = leitura.Split('-');
                    if (tokensFinais.Length == 2)
                    {

                        var frameLido = ParserAntena.ParseData(tokensFinais[0]);
                        if (frameLido != null)
                        {
                            if (frameLido.TipoFrame == TrataFrames.LeituraTag)
                            {
                                if (instanteMillis == null || instanteMillis.Trim() != tokensFinais[1].Trim())
                                {
                                    //TOdo IMPLEMENTAR TAMBÉM A VERIFICAÇÃO DE DATA E HORA PELO APP PRA EVITAR MULTIPLOS DISPAROS EM UM MESMO INSTANTE
                       
                                    frame = (frameLido as FrameLeituraTag);
                                    frame.IDMensagem = tokensFinais[1];
                                    _ = this.DisplayToastAsync($"Tag lida: {frame.TagID} ", 800);

                                    navegacao.SetLocal(frame.TagID);

                                }



                            }
                        }
                    }

                }
            }
            catch (Exception err)
            {
               
            }
            return frame==null ?null : (FrameLeituraTag)frame.Clone();
        }

        private async void ProcurarDispositivo()
        {
            if (_bluetoothService != null)
            {
                _=TTSHelper.Speak("Procurando dispositivo");    
                if (_bluetoothService is IAndroidBluetoothService)
                {
                    (_bluetoothService as IAndroidBluetoothService).OnBluetoothScanTerminado -= MainPage_OnBluetoothScanTerminado;
                    (_bluetoothService as IAndroidBluetoothService).OnBluetoothScanTerminado += MainPage_OnBluetoothScanTerminado;
                    _ = _bluetoothService.EscanearDispositivosAsync();
                    _ = this.DisplayToastAsync("Procurando dispositivos..", 2000);
                }


            }
        }
        private async void btn_escanearBluetooth_Clicked(object sender, EventArgs e)
        {
            //await Navigation.PushAsync(new DispositivosBluetooth(_bluetoothService));

            

        }

        private async void MainPage_OnBluetoothScanTerminado()
        {
            List<IDevice> dispositivos = new List<IDevice>();
            if (_bluetoothService is IAndroidBluetoothService)
                dispositivos = new List<IDevice>((_bluetoothService as IAndroidBluetoothService)._dispositivosEscaneados);

            IDevice deviceMaiorRSSI = null;
            if (dispositivos != null && dispositivos.Count > 0)
            {
                (_bluetoothService as IAndroidBluetoothService).OnBluetoothScanTerminado -= MainPage_OnBluetoothScanTerminado;

                foreach (IDevice device in dispositivos)
                {
                    if (deviceMaiorRSSI == null || deviceMaiorRSSI.Rssi > device.Rssi)
                        deviceMaiorRSSI = device;

                }

                if (_device != null)
                    _device.Dispose();

                if (deviceMaiorRSSI != null)
                {
                    _=this.DisplayToastAsync($"Dispositivo encontrado! {deviceMaiorRSSI.Name}", 800);
                    if (await StorageDAO.SalvaConfiguracoesNomeBengala(deviceMaiorRSSI.Name))
                        ConectarNaBengala();

                }
                else
                    _ = this.DisplayToastAsync($"Nenhum dispositivo encontrado!", 800);


            }
        }

        private  void btn_vibrarMotor_Clicked(object sender, EventArgs e)
        {
            FilaRequsicoesBengala.Add(new RequisicaoMotor() { Tipo = Enum.EnumTipoRequisicaoBengala.AcionarMotor, QtVibracoes = 2 });
           

        }

        private async Task<bool> VibrarMotor(int qtVibracao)
        {
            return await _bluetoothService.AcionarVibracaoBengala(_device, qtVibracao);
        }

        private void SwipeGestureRecognizer_Swiped(object sender, SwipedEventArgs e)
        {

            Console.WriteLine("Swipe Direita");
            FilaGestos.Enqueue(new GestoSwipeDireita());
            Console.WriteLine("Swipe adicionado!");

        }

        private void SwipeGestureRecognizer_Swiped_1(object sender, SwipedEventArgs e)
        {
            Console.WriteLine("Swipe Baixo");
            FilaGestos.Enqueue(new GestoSwipeBaixo());
            Console.WriteLine("Swipe Baixo!");
        }

        private void SwipeGestureRecognizer_Swiped_2(object sender, SwipedEventArgs e)
        {
            Console.WriteLine("Swipe Cima");
            FilaGestos.Enqueue(new GestoSwipeCima());
            Console.WriteLine("Swipe Cima!");
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            navegacao.CalcularRota("Luga3");
        }
        //Swipe para direita

    }
}
