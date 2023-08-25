using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace GuideMe
{
    public static class TTSHelper
    {
        public static SpeechOptions ConfiguracoesFala { get; private set; } = null;
        public static async Task<bool> Speak(string text)
        {
            bool retorno = false;

            try
            {
                if (ConfiguracoesFala == null)
                    await ConfigurarServicoVozAndroid();

                await TextToSpeech.SpeakAsync(text, ConfiguracoesFala);
                retorno = true;
            }
            catch (Exception)
            { 

            }
            return retorno;
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
