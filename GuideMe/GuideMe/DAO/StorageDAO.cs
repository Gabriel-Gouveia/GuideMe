using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GuideMe.DAO
{
    public static class StorageDAO
    {
        private const string KEY_BLUETOOTH_BENGALA_NAME = "KEY_BLUETOOTH_BENGALA_NAME";

        private static string _NomeBengalaBluetooth = null;
        public static string NomeBengalaBluetooth
        {
            get
            {
                if (string.IsNullOrEmpty(_NomeBengalaBluetooth))
                    _NomeBengalaBluetooth= ObtemConfiguracoesNomeBengala();

                return _NomeBengalaBluetooth;
            }

        }

        public static async Task<bool> SalvaConfiguracoesNomeBengala(string nome)
        {
            try
            {
                await SecureStorage.SetAsync(KEY_BLUETOOTH_BENGALA_NAME, nome);
                return true;
            }
            catch (Exception erro)
            {
                
            }
            return false;
        }
        private static string ObtemConfiguracoesNomeBengala()
        {
            try
            {
                string resultado = SecureStorage.GetAsync(KEY_BLUETOOTH_BENGALA_NAME).Result;
                return resultado;
            }
            catch (Exception erro)
            {

            }
            return null;
        }

    }
}
