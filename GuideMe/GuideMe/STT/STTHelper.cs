using GuideMe.Enum;
using GuideMe.Interfaces;
using Microsoft.CognitiveServices.Speech;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace GuideMe.STT
{
    public static class STTHelper
    {
        public class ComandoVozEventArgs : EventArgs
        {
            public double Threshold { get; set; }
            public DateTime TimeReached { get; private set; }
            public EnumComandoVoz Comando { get; set; }

            public string Auxiliar { get; set; }
            public string Lugar { get; set; }

            public ComandoVozEventArgs(double threshold, EnumComandoVoz comando,string auxiliar,string lugar)
            {
                this.Threshold = threshold;
                this.Comando = comando;
                this.TimeReached = DateTime.Now;
                this.Auxiliar = auxiliar;
                this.Lugar = lugar;
            }
        }
        static Dictionary<string, string> ComandoVoz_Lugar = new Dictionary<string, string>();
        static SpeechRecognizer recognizer;
        public static IMicrophoneService micService { get; private set; }
        public static string SubscriptionKey { get; private set; } = "daac6ced6fbc40f3a22528445a208b2c";
        public static string Region { get; set; } = "brazilsouth";
        public static string Language { get; set; } = "pt-BR";
        public static bool MicEnabled { get; private set; } = false;
        public static bool IsTranscribbing { get; private set; } = false;

        public static EventHandler<SpeechRecognitionEventArgs> OnRecognizedSomething;
        public static EventHandler<ComandoVozEventArgs> OnComandoVozDetectado;
        public static double Threshold { get; set; } = 0.7;

        public static Dictionary<EnumComandoVoz, List<string>> ComandosVoz = null;

      
        static string[] VariarString(string pattern, string[] argumentos)
        {
            string[] retornos = new string[argumentos.Length];

            for (int n = 0; n < argumentos.Length; n++)
                retornos[n] = string.Format(pattern, argumentos[n]);

            return retornos;
        }

        static double GetSimiliaridade(string primeira, string segunda)
        {
            double porcentagem = 0.0;
            string maior;
            string menor;

            if (primeira.ToLower() == segunda.ToLower())
                return 1;

            if (primeira.Length > segunda.Length)
            {
                maior = primeira;
                menor = segunda;
            }
            else if (segunda.Length > primeira.Length)
            {
                maior = segunda;
                menor = primeira;
            }
            else
            {
                maior = primeira;
                menor = segunda;
            }
            maior = maior.ToLower();
            menor = menor.ToLower();
            int erros = 0;
            for (int n = 0; n < menor.Length; n++)
            {
                if (maior[n] != menor[n])
                    erros++;
            }



            if (maior.Length == menor.Length)
            {
                if (erros != 0)
                    return (double)(maior.Length - erros) / (double)erros;
                else
                    return 1;

            }
            else
            {
                erros += maior.Length - menor.Length;
                return (double)(maior.Length-erros) / (double)erros;
            }




            return porcentagem;

        }
        static string[] GetTokens(string falaBruta)
        {
            return falaBruta.Split(' ');
        }

        public static async void InitService()
        {
            micService = DependencyService.Resolve<IMicrophoneService>();
            MicEnabled = await micService.GetPermissionAsync();
            if (MicEnabled)
                InitializeSpeech();
        }
        private static List<string> GetPatternsTestarDispositivo()
        {
            List<string> retorno = new List<string>();
            string pattern1 = "Testar {0} dispositivo";
            string[] variacoes1 = VariarString(pattern1, new string[] { "", "o", "os" });
            foreach (string s in variacoes1)
                retorno.Add(s);

            string pattern2 = "Teste {0} dispositivo";
            variacoes1 = VariarString(pattern2, new string[] { "", "o", "os" });

            foreach (string s in variacoes1)
                retorno.Add(s);

            return retorno;

        }
        private static List<string> GetPatternsListarLugares()
        {
            List<string> retorno = new List<string>();
            string pattern1 = "Me fale {0} lugares";
            string[] variacoes1 = VariarString(pattern1, new string[] { "", "o", "os","a","as" });
            foreach (string s in variacoes1)
                retorno.Add(s);

            string pattern2 = "Quais são {0} lugares disponiveis";
            variacoes1 = VariarString(pattern2, new string[] { "", "o", "os","a","as" });

            foreach (string s in variacoes1)
                retorno.Add(s);

            return retorno;

        }
        private static void PreencheComandosVoz()
        {
            if (ComandosVoz == null)
            {
                ComandosVoz = new Dictionary<EnumComandoVoz, List<string>>();
                ComandosVoz.Add(EnumComandoVoz.TestarDispositivo, GetPatternsTestarDispositivo());
                ComandosVoz.Add(EnumComandoVoz.ListarLugares, GetPatternsListarLugares());
            }
            
        }

        private static List<string> GetPatternsIrPara(List<string> lugares)
        {
            ComandoVoz_Lugar = new Dictionary<string, string>();
            List<string> retorno = new List<string>();
            string pattern1 = "Ir para {0}";
            string[] variacoes1 = VariarString(pattern1, new string[] { "", "o", "a", });
            foreach (string s in variacoes1)
                foreach (string lugar in lugares)
                {
                    retorno.Add(s + $" {lugar}");
                    ComandoVoz_Lugar.Add(s + $" {lugar}", lugar);
                }
                    

            string pattern2 = "quero ir {0}";
            variacoes1 = VariarString(pattern2, new string[] { "", "para o", "para a","ao","na","nas","no" });

            foreach (string s in variacoes1)
                foreach (string lugar in lugares)
                {
                    retorno.Add(s + $" {lugar}");
                    ComandoVoz_Lugar.Add(s + $" {lugar}", lugar);
                }

            return retorno;

        }
        public static void RegistrarLugares(List<string> lugares)
        {

            PreencheComandosVoz();
            if (ComandosVoz.ContainsKey(EnumComandoVoz.Irpara))
                ComandosVoz[EnumComandoVoz.Irpara] = GetPatternsIrPara(lugares);
            else
                ComandosVoz.Add(EnumComandoVoz.Irpara, GetPatternsIrPara(lugares));

        }
        private static void ProcessarAudio(string fala)
        {
            foreach (KeyValuePair<EnumComandoVoz, List<string>> value in ComandosVoz)
            {
                double limite = 0.7;
                if (value.Key == EnumComandoVoz.Irpara)
                    limite = 0.9;
                
                foreach (string s in value.Value)
                {
                    double threshold = GetSimiliaridade(fala.ToLower(), s.ToLower());
                    if (threshold >= limite)
                    {
                        string lugar = "";
                        if (value.Key == EnumComandoVoz.Irpara)
                        {
                            if (ComandoVoz_Lugar.ContainsKey(s))
                            {
                                lugar = ComandoVoz_Lugar[s];
                                if (fala.ToLower().Contains(lugar.ToLower()))
                                {
                                    OnComandoVozDetectado?.Invoke(new object(),
                                       new ComandoVozEventArgs(threshold, value.Key, s, lugar));
                                    StopListening();
                                    break;
                                }
                                   
                            }
                               

                        }
                        else
                        {
                            OnComandoVozDetectado?.Invoke(new object(),
                             new ComandoVozEventArgs(threshold, value.Key, s, lugar));
                            StopListening();
                            break;

                        }

                        

                        
                    }
                }
            }
        }
        private static void InitializeSpeech()
        {
            PreencheComandosVoz();
            // initialize speech recognizer 
            if (recognizer == null)
            {
                var config = SpeechConfig.FromSubscription(SubscriptionKey, Region);
                recognizer = new SpeechRecognizer(config, Language);
                recognizer.Recognized += (obj, args) =>
                {
                    _ = Task.Factory.StartNew(_ => ProcessarAudio(args.Result.Text), TaskCreationOptions.LongRunning);

                    Console.WriteLine($"Recognized: {args.Result.Text}");
                    if (OnRecognizedSomething != null)
                        OnRecognizedSomething.Invoke(obj, args);
                };
            }
        }
        public static async void StartListening()
        {
            if (!MicEnabled)
                MicEnabled = await micService.GetPermissionAsync();

            if (MicEnabled)
            {
                InitializeSpeech();
                if (!IsTranscribbing)
                {
                    await TTSHelper.Speak("Me diga o que você precisa");

                    _ = Task.Factory.StartNew(_ => Listen(), TaskCreationOptions.LongRunning);

                    Thread.Sleep(100);
                    _ = Task.Factory.StartNew(_ => TimeOutListen(), TaskCreationOptions.LongRunning);
                }
                 

            }
        }
        private static void TimeOutListen()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (IsTranscribbing && sw.ElapsedMilliseconds <= 8000)
                Thread.Sleep(100);

            if (IsTranscribbing)
                StopListening();
        }
        public static async void StopListening()
        {
            await recognizer.StopContinuousRecognitionAsync();
            IsTranscribbing = false;
        }
        private static async void Listen()
        {
            IsTranscribbing = true;
            await recognizer.StartContinuousRecognitionAsync();
        }
        private static async void Transcrever()
        {
            if (!MicEnabled)
                MicEnabled = await micService.GetPermissionAsync();

            // EARLY OUT: make sure mic is accessible
            if (!MicEnabled)
            {
                _ = TTSHelper.Speak("Não possuo acesso ao microfone");
                return;
            }

            // initialize speech recognizer 
            if (recognizer == null)
            {
                var config = SpeechConfig.FromSubscription(SubscriptionKey, Region);
                recognizer = new SpeechRecognizer(config, Language);
                recognizer.Recognized += (obj, args) =>
                {
                    if (OnRecognizedSomething != null)
                        OnRecognizedSomething.Invoke(obj, args);
                };
            }


        }
    }
}
