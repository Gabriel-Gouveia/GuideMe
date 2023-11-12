using GuideMe.Enum;
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
        private const string SiteURL = "http://192.168.64.222:5254";
        private const string LoginApi = "/api/Login/v1/login";
        private const string GetTagsEstabelecimento = "/api/Tag/v1/GetTagData";
        private static string tokenAPI = "";
        private static HttpClient client = null;
        private static HttpClientHandler httpClientHandler = null;
        private static LoginRequestTO _loginRequest;
        private static bool _MocarDados = true;


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
        private static EstabelecimentoTagsTO GetMockDados()
        {
            EstabelecimentoTagsTO retorno = new EstabelecimentoTagsTO();

            retorno.NomeEstabelecimento = "Centro de Apoio ao Deficiente Visual";

            List<TagTO> Tags = new List<TagTO>();

            TagTO tag1 = new TagTO();
            tag1.Id = 16;
            tag1.TagId = "E2801191A502000000155008";
            tag1.EstabelecimentoId = 16;
            tag1.tipoTag = (int)EnumTipoTag.lugar;
            tag1.Nome = "tagRecepção";
            tag1.TagsPai = new List<TagsPaiTO>();

            TagTO tag2 = new TagTO();
            tag2.Id = 17;
            tag2.TagId = "E2801191A502000000155009";
            tag2.EstabelecimentoId = 16;
            tag2.tipoTag = (int)EnumTipoTag.localizacao;
            tag2.Nome = "tag02";
            tag2.TagsPai = new List<TagsPaiTO>();

            TagTO tag3 = new TagTO();
            tag3.Id = 18;
            tag3.TagId = "E2801191A502000000155010";
            tag3.EstabelecimentoId = 16;
            tag3.tipoTag = (int)EnumTipoTag.localizacao;
            tag3.Nome = "tag03";
            tag3.TagsPai = new List<TagsPaiTO>();

            TagTO tag4 = new TagTO();
            tag4.Id = 20;
            tag4.TagId = "E2801191A502000000155014";
            tag4.EstabelecimentoId = 16;
            tag4.tipoTag = (int)EnumTipoTag.localizacao;
            tag4.Nome = "tag04";
            tag4.TagsPai = new List<TagsPaiTO>();

            TagTO tag5 = new TagTO();
            tag5.Id = 21;
            tag5.TagId = "E2801191A502000000155012";
            tag5.EstabelecimentoId = 16;
            tag5.tipoTag = (int)EnumTipoTag.localizacao;
            tag5.Nome = "tag05";
            tag5.TagsPai = new List<TagsPaiTO>();

            TagTO tag6 = new TagTO();
            tag6.Id = 22;
            tag6.TagId = "E2801191A502000000155013";
            tag6.EstabelecimentoId = 16;
            tag6.tipoTag = (int)EnumTipoTag.localizacao;
            tag6.Nome = "tagBanheiro";
            tag6.TagsPai = new List<TagsPaiTO>();

            TagTO tag7 = new TagTO();
            tag7.Id = 23;
            tag7.TagId = "E2801191A502000000155015";
            tag7.EstabelecimentoId = 16;
            tag7.tipoTag = (int)EnumTipoTag.localizacao;
            tag7.Nome = "tagBanheiro";
            tag7.TagsPai = new List<TagsPaiTO>();

            TagTO tagSala1 = new TagTO();
            tagSala1.Id = 24;
            tagSala1.TagId = "E2801191A502000000155018";
            tagSala1.EstabelecimentoId = 16;
            tagSala1.tipoTag = (int)EnumTipoTag.lugar;
            tagSala1.Nome = "tagBanheiro";
            tagSala1.TagsPai = new List<TagsPaiTO>();

            TagTO tag8 = new TagTO();
            tag8.Id = 25;
            tag8.TagId = "E2801191A502000000155097";
            tag8.EstabelecimentoId = 16;
            tag8.tipoTag = (int)EnumTipoTag.localizacao;
            tag8.Nome = "tagBanheiro";
            tag8.TagsPai = new List<TagsPaiTO>();
            //ida tag_pai é o menor id e o outro o maior
            tag1.TagsPai.Add(new TagsPaiTO() { Id_Tag_Pai = tag1.Id, Id_Tag = tag2.Id, Direcao = (int)EnumDirecao.Esquerda });
            tag2.TagsPai.Add(new TagsPaiTO() { Id_Tag_Pai = tag1.Id, Id_Tag = tag2.Id, Direcao = (int)EnumDirecao.Esquerda });
            tag1.TagsPai.Add(new TagsPaiTO() { Id_Tag_Pai = tag1.Id, Id_Tag = tag7.Id, Direcao = (int)EnumDirecao.Direita });
            tag7.TagsPai.Add(new TagsPaiTO() { Id_Tag_Pai = tag1.Id, Id_Tag = tag7.Id, Direcao = (int)EnumDirecao.Direita });

            tag2.TagsPai.Add(new TagsPaiTO() { Id_Tag_Pai = tag2.Id, Id_Tag = tag3.Id, Direcao = (int)EnumDirecao.Frente });
            tag3.TagsPai.Add(new TagsPaiTO() { Id_Tag_Pai = tag2.Id, Id_Tag = tag3.Id, Direcao = (int)EnumDirecao.Frente });

            tag3.TagsPai.Add(new TagsPaiTO() { Id_Tag_Pai = tag3.Id, Id_Tag = tag4.Id, Direcao = (int)EnumDirecao.Frente });
            tag4.TagsPai.Add(new TagsPaiTO() { Id_Tag_Pai = tag3.Id, Id_Tag = tag4.Id, Direcao = (int)EnumDirecao.Frente });

            tag4.TagsPai.Add(new TagsPaiTO() { Id_Tag_Pai = tag4.Id, Id_Tag = tag5.Id, Direcao = (int)EnumDirecao.Esquerda });
            tag5.TagsPai.Add(new TagsPaiTO() { Id_Tag_Pai = tag4.Id, Id_Tag = tag5.Id, Direcao = (int)EnumDirecao.Esquerda });

            tag5.TagsPai.Add(new TagsPaiTO() { Id_Tag_Pai = tag5.Id, Id_Tag = tag6.Id, Direcao = (int)EnumDirecao.Esquerda });
            tag6.TagsPai.Add(new TagsPaiTO() { Id_Tag_Pai = tag5.Id, Id_Tag = tag6.Id, Direcao = (int)EnumDirecao.Esquerda });

            tag7.TagsPai.Add(new TagsPaiTO() { Id_Tag_Pai = tag7.Id, Id_Tag = tagSala1.Id, Direcao = (int)EnumDirecao.Frente });
            tagSala1.TagsPai.Add(new TagsPaiTO() { Id_Tag_Pai = tag7.Id, Id_Tag = tagSala1.Id, Direcao = (int)EnumDirecao.Frente });

            tagSala1.TagsPai.Add(new TagsPaiTO() { Id_Tag_Pai = tagSala1.Id, Id_Tag = tag8.Id, Direcao = (int)EnumDirecao.Frente });
            tag8.TagsPai.Add(new TagsPaiTO() { Id_Tag_Pai = tagSala1.Id, Id_Tag = tag8.Id, Direcao = (int)EnumDirecao.Frente });

            tag8.TagsPai.Add(new TagsPaiTO() { Id_Tag_Pai = tag5.Id, Id_Tag = tag8.Id, Direcao = (int)EnumDirecao.Esquerda });
            tag5.TagsPai.Add(new TagsPaiTO() { Id_Tag_Pai = tag5.Id, Id_Tag = tag8.Id, Direcao = (int)EnumDirecao.Esquerda });

            Tags.Add(tag1);
            Tags.Add(tag2);
            Tags.Add(tag3);
            Tags.Add(tag4);
            Tags.Add(tag5);
            Tags.Add(tag6);
            Tags.Add(tag7);
            Tags.Add(tagSala1);
            Tags.Add(tag8);





            List<LugaresTO> Lugares = new List<LugaresTO>();

            LugaresTO lugar1=new LugaresTO();

            lugar1.TAG_id = tag6.Id;
            lugar1.Nome = "Banheiro";
            lugar1.Descricao = "";
            lugar1.Navegavel = true;

            LugaresTO lugar2 = new LugaresTO();

            lugar2.TAG_id = tag1.Id;
            lugar2.Nome = "Recepção";
            lugar2.Descricao = "";
            lugar2.Navegavel = true;

            LugaresTO Sala1 = new LugaresTO();

            Sala1.TAG_id = tagSala1.Id;
            Sala1.Nome = "Sala um";
            Sala1.Descricao = "";
            Sala1.Navegavel = true;


            Lugares.Add(lugar2);
            Lugares.Add(lugar1);
            Lugares.Add(Sala1);

            List<ItensTO> Itens = new List<ItensTO>();

            retorno.Tags = Tags;
            retorno.Lugares = Lugares;
            retorno.Itens = Itens;



            return retorno;
        }

        public static EstabelecimentoTagsTO GetEstabelecimentoTags(string tagId)
        {
            EstabelecimentoTagsTO data=null;
            try
            {
                if (!_MocarDados)
                {
                    var retornoAPI = ExecGetAPI(MontaLinkAPI(GetTagsEstabelecimento, "TagID", tagId), null, 3, timeOutEmSegundos: 5, anonymous: false);
                    if (retornoAPI.Sucesso)
                    {
                        data = JsonConvert.DeserializeObject<EstabelecimentoTagsTO>(retornoAPI.RetornoObj.ToString());
                    }
                }
                else
                    data = GetMockDados();



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
