using GuideMe.TOs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace GuideMe.Utils
{
    static public class APIHelper
    {
        //private const string SiteURL = "https://guideme.azurewebsites.net";
        private const string SiteURL = "http://192.168.0.133:5254";
        private const string LoginApi = "/api/Login/v1/login";
        private const string GetTagsEstabelecimento = "/api/Tag/v1/GetTagData";
        private static string tokenAPI = "";
        private static HttpClient client = null;
        private static HttpClientHandler httpClientHandler = null;
        private static LoginRequestTO _loginRequest;

        private static HttpClient VerificaHttpClient()
        {
            if (client != null)
                return client;

            if (Debugger.IsAttached)
            {
                httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback =
                (message, cert, chain, errors) => { return true; };
                client = new HttpClient(httpClientHandler);
            }
            else
            {
                client = new HttpClient();
                return client;
            }

            return client;
        }

        public static EstabelecimentoTagsTO GetEstabelecimentoTags(string tagId)
        {
            EstabelecimentoTagsTO data=null;
            try
            {
                var retornoAPI = ExecGetAPI(MontaLinkAPI(GetTagsEstabelecimento, "TagID",tagId), null, 3, anonymous: false);
                if (retornoAPI.Sucesso)
                {
                    data = JsonConvert.DeserializeObject<EstabelecimentoTagsTO>(retornoAPI.RetornoObj.ToString());
                }

            }
            catch (Exception err)
            {

            }

            return data;

        }

        private static string MontaLinkAPI(string api,string parametro=null, string valorPrametro=null)
        {
            if(string.IsNullOrEmpty(parametro) && string.IsNullOrEmpty(valorPrametro))
                return SiteURL + api;
            else
                return SiteURL + api + $"?{parametro}={valorPrametro.Replace(" ","")}";
        }
        public static LoginResponseTO GetAuthenticationToken(LoginRequestTO loginRequest)
        {
            LoginResponseTO response = new LoginResponseTO();
            ResultCallApi resultado = new ResultCallApi();
            try
            {
                _loginRequest = loginRequest;
                resultado = ExecPostAPI(MontaLinkAPI(LoginApi), loginRequest, 3);

                if (resultado != null && resultado.Sucesso)
                {
                    var teste = JsonConvert.DeserializeObject<LoginResponseRoot>(resultado.Retorno);
                    tokenAPI = teste.loginResponse.Token;
                    response = teste.loginResponse;
                }
            }
            catch (Exception e)
            {
            }


            return response;
        }
        public static ResultCallApi ExecPostAPI(string api, object objParaEnviar, int tentativas, int timeOutEmSegundos = 100,
            bool anonymous = true)
        {
            ResultCallApi retorno = new ResultCallApi();
            retorno.Retorno = string.Empty;
            _loginRequest = new LoginRequestTO();
            _loginRequest.UserName = UserSecretsManager.Settings["login"];
            _loginRequest.Password = UserSecretsManager.Settings["senha"];

            try
            {
                LoginResponseTO token = new LoginResponseTO();
                Stopwatch sw = new Stopwatch();
                Random gerador = new Random();
                int handleTentativa = gerador.Next(1, 9999999);
                for (int n = 1; n <= tentativas; n++)
                {
                    sw.Restart();
                    using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, api))
                    {
                        if (!anonymous)
                        {
                            if (string.IsNullOrEmpty(tokenAPI))
                            {
                                if (_loginRequest == null)
                                    return retorno;

                                var autenticacao = GetAuthenticationToken(_loginRequest);
                                if (autenticacao != null && !string.IsNullOrEmpty(autenticacao.Token))
                                    tokenAPI = autenticacao.Token;

                            }
                            else
                            {
                                VerificaHttpClient().DefaultRequestHeaders.Clear();
                                VerificaHttpClient().DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAPI);
                            }
                        }
                        if (!anonymous && !string.IsNullOrEmpty(tokenAPI) || anonymous)
                        {
                            string dadoSerializado = JsonConvert.SerializeObject(objParaEnviar);

                            request.Content = new StringContent(dadoSerializado, Encoding.UTF8, "application/json");

                            var response = VerificaHttpClient().SendAsync(request).Result;


                            if (response.IsSuccessStatusCode)
                            {
                                retorno.Sucesso = true;
                                retorno.Retorno = response.Content.ReadAsStringAsync().Result;
                                //retorno.Retorno = TrataRespostaApi(retorno.Retorno);
                                break;
                            }
                            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && !anonymous && !string.IsNullOrEmpty(tokenAPI))
                            {
                                var autenticacao = GetAuthenticationToken(_loginRequest);
                                if (autenticacao != null && !string.IsNullOrEmpty(autenticacao.Token))
                                    tokenAPI = autenticacao.Token;
                            }

                        }




                    }
                }
            }
            catch (Exception ex)
            {
                retorno.Erro = ex;
            }

            return retorno; ;
        }



        public static ResultCallApi ExecGetAPI(string api, object objParaEnviar, int tentativas, int timeOutEmSegundos = 100,
            bool anonymous = true)
        {
            ResultCallApi retorno = new ResultCallApi();
            retorno.Retorno = string.Empty;
            _loginRequest = new LoginRequestTO();
            _loginRequest.UserName = UserSecretsManager.Settings["login"];
            _loginRequest.Password = UserSecretsManager.Settings["senha"];

            try
            {
                LoginResponseTO token = new LoginResponseTO();
                Stopwatch sw = new Stopwatch();
                Random gerador = new Random();
                int handleTentativa = gerador.Next(1, 9999999);
                for (int n = 1; n <= tentativas; n++)
                {
                    sw.Restart();
                    using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, api))
                    {
                        if (!anonymous)
                        {
                            if (string.IsNullOrEmpty(tokenAPI))
                            {
                                if (_loginRequest == null)
                                    return retorno;

                                var autenticacao = GetAuthenticationToken(_loginRequest);
                                if (autenticacao != null && !string.IsNullOrEmpty(autenticacao.Token))
                                    tokenAPI = autenticacao.Token;

                            }
                            else
                            {
                                VerificaHttpClient().DefaultRequestHeaders.Clear();
                                VerificaHttpClient().DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAPI);
                            }
                        }

                        if (!anonymous && !string.IsNullOrEmpty(tokenAPI) || anonymous)
                        {

                            string dadoSerializado = JsonConvert.SerializeObject(objParaEnviar);

                            request.Content = new StringContent(dadoSerializado, Encoding.UTF8, "application/json");

                            var response = VerificaHttpClient().SendAsync(request).Result;

                            if (response.IsSuccessStatusCode)
                            {

                                string obj = response.Content.ReadAsStringAsync().Result;
                                ResultCallApi result = new ResultCallApi();
                                result.Sucesso = true;
                                result.RetornoObj = JsonConvert.DeserializeObject<dynamic>(obj);
                                retorno.Sucesso = true;
                                retorno.Retorno = result.Retorno;
                                retorno.RetornoObj = result.RetornoObj;
                                //retorno.Retorno = TrataRespostaApi(retorno.Retorno);
                                break;
                            }
                            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && !anonymous && !string.IsNullOrEmpty(tokenAPI))
                            {
                                var autenticacao = GetAuthenticationToken(_loginRequest);
                                if (autenticacao != null && !string.IsNullOrEmpty(autenticacao.Token))
                                    tokenAPI = autenticacao.Token;
                            }

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                retorno.Erro = ex;
            }

            return retorno; ;
        }
    }
}
