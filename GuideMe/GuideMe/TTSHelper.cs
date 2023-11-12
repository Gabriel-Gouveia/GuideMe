using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace GuideMe
{
    public static class TTSHelper
    {
        public static SpeechOptions ConfiguracoesFala { get; private set; } = null;
        private static string UltimaFala = null;
        private static Stopwatch swUltimaFala = new Stopwatch();
        private static bool _lockFalando = false;
        private static Dictionary<string, DateTime> filtroFalas = new Dictionary<string, DateTime>();
        public static async Task<bool> Speak(string text)
        {
            bool retorno = false;

            try
            {
                /*while (!_lockFalando)
                    Thread.Sleep(100);*/

                _lockFalando = true;

                if (ConfiguracoesFala == null)
                    await ConfigurarServicoVozAndroid();

                /* if (!filtroFalas.ContainsKey(text))
                 {
                     filtroFalas.Add(text, DateTime.Now);
                     await TextToSpeech.SpeakAsync(text, ConfiguracoesFala);
                     LimparFiltro();
                     retorno = true;
                 }
                 else if (filtroFalas[text].Subtract(DateTime.Now).TotalSeconds >= 3)
                 {
                     await TextToSpeech.SpeakAsync(text, ConfiguracoesFala);
                     LimparFiltro();
                     retorno = true;
                 }*/
                await TextToSpeech.SpeakAsync(text, ConfiguracoesFala);

            }
            catch (Exception)
            {

            }
            finally
            {
                _lockFalando = false;
            }
            return retorno;
        }

        private static void LimparFiltro()
        {
            List<string> dadosRemover = new List<string>();
            foreach (KeyValuePair<string, DateTime> valor in filtroFalas)
            {
                if (valor.Value.Subtract(DateTime.Now).TotalSeconds >= 3)
                    dadosRemover.Add(valor.Key);
            }

            foreach (string s in dadosRemover)
                if (filtroFalas.ContainsKey(s))
                    filtroFalas.Remove(s);
        }

        private static async Task<bool> ConfigurarServicoVozAndroid()
        {
            bool retorno = true;

            try
            {
                if (ConfiguracoesFala == null)
                {
                    var locales = await TextToSpeech.GetLocalesAsync();
                    Locale local = null;

                    local = locales.First(x => x.Country == "BR");

                    if (local == null)
                        local = locales.First(x => x.Country == "PT");

                    if (local == null)
                        local = locales.FirstOrDefault();

                    ConfiguracoesFala = new SpeechOptions()
                    {
                        Volume = .75f,
                        Pitch = 1.0f,
                        Locale = local
                    };


                }
            }
            catch (Exception ex)
            {
                retorno = false;
            }

            return retorno;
        }
    }
}
