using GuideMe.Enum;
using GuideMe.Interfaces;
using Microsoft.CognitiveServices.Speech;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace GuideMe.STT
{
    public static class STTHelper
    {
        public static ContentPage Page { get; set; }
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
        public static EventHandler<ComandoVozEventArgs> OnComandoVozDetectadoDebugger;
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

                if (Debugger.IsAttached)
                    Console.WriteLine($"************************************* GetSimiliaridade {primeira} {segunda} {(double)(maior.Length - erros) / (double)erros}");

                return (double)(maior.Length-erros) / (double)erros;
            }

            if (Debugger.IsAttached)
                Console.WriteLine($"************************************* GetSimiliaridade {primeira} {segunda} {porcentagem}");


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
                    ComandoVoz_Lugar.Add(s + $" {lugar.ToLower()}", lugar);
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
        private static List<string> GetPatternsOndeEstou()
        {         
            List<string> retorno = new List<string>();
            retorno.Add("Onde eu estou ?");
            retorno.Add("Onde eu to ?");
            retorno.Add("Aonde eu estou ?");
            retorno.Add("Aonde eu to ?");
            retorno.Add("Onde eu estou");
            retorno.Add("Onde eu to");
            retorno.Add("Aonde eu estou");
            retorno.Add("Aonde eu to");

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
        public static void RegistrarOndeEstou()
        {
            PreencheComandosVoz();
            if (ComandosVoz.ContainsKey(EnumComandoVoz.OndeEstou))
                ComandosVoz[EnumComandoVoz.OndeEstou] = GetPatternsOndeEstou();
            else
                ComandosVoz.Add(EnumComandoVoz.OndeEstou, GetPatternsOndeEstou());
        }
        static string NormalizarString(string text)
        {
            return RemoveAcentos(text).ToLower();
        }
        static string RemoveAcentos(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC);
        }

        private static void ProcessarAudio(string fala)
        {
            if (Debugger.IsAttached)
            {
                OnComandoVozDetectadoDebugger?.Invoke(new object(),
                                       new ComandoVozEventArgs(0.0, EnumComandoVoz.TestarDispositivo,
                                       fala, fala));
            }
            if (Debugger.IsAttached)
                Console.WriteLine($"************************************* ProcessarAudio {fala}");

            foreach (KeyValuePair<EnumComandoVoz, List<string>> value in ComandosVoz)
            {
                
                double limite = 0.7;
                if (value.Key == EnumComandoVoz.Irpara)
                    limite = 0.7;
                
                foreach (string s in value.Value)
                {
                    if (Debugger.IsAttached)
                        Console.WriteLine($"************************************* ProcessarAudio procurando {s}");
                    double threshold = GetSimiliaridade(fala.ToLower(), s.ToLower());
                    if (threshold >= limite)
                    {
                        if (Debugger.IsAttached)
                            Console.WriteLine($"************************************* ProcessarAudio limite alcançado! {threshold}");

                        string lugar = "";
                        if (value.Key == EnumComandoVoz.Irpara)
                        {

                            if (Debugger.IsAttached)
                                Console.WriteLine($"************************************* ProcessarAudio é um comando de local!");

                            if (ComandoVoz_Lugar.ContainsKey(s))
                            {
                                if (Debugger.IsAttached)
                                    Console.WriteLine($"************************************* ProcessarAudio chave encontrada!");

                                lugar = ComandoVoz_Lugar[s];
                                string falaNormalizada = NormalizarString(fala);
                                string lugarNormalizado = NormalizarString(lugar);
                                if (falaNormalizada.Contains(lugarNormalizado))
                                {
                                    if (Debugger.IsAttached)
                                        Console.WriteLine($"************************************* ProcessarAudio fala contém o lugar realmente!");

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

                    _ = Task.Factory.StartNew(_ => Listen(), TaskCreationOptions.AttachedToParent);

                    Thread.Sleep(100);
                    _ = Task.Factory.StartNew(_ => TimeOutListen(), TaskCreationOptions.AttachedToParent);
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
       
    }
}
