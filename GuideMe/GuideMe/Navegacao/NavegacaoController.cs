using GuideMe.Enum;
using GuideMe.STT;
using GuideMe.TOs;
using GuideMe.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GuideMe.Navegacao
{
    public class NavegacaoController
    {
        public EstabelecimentoTagsTO DadosEstabelecimento { get; private set; }

        private GraphDFS grafo;
        public bool LocalDefinido { get; set; }

        private Stopwatch swDescreverLugar = new Stopwatch();

        public TagTO UltimaTagLida { get; set; }
        public TagTO UltimaTagQueFoiDescrita { get; set; }
        public List<LugaresTO> All_Lugares { get; private set; }
        public List<LugaresTO> All_Lugares_Navegaveis { get; private set; }
        public List<string> All_Lugares_Navegaveis_nome { get; private set; }
        public List<ItensTO> All_Itens { get; private set; }
        public List<ItensTO> All_Itens_Navegaveis { get; private set; }

        public List<TagTO> RotaAtual { get; private set; }
        public int PosAtualRota { get; private set; }
        private string _lugarDesejado = null;

        private bool GetDadosFromServer(string tag)
        {
            try
            {
                var data = APIHelper.GetEstabelecimentoTags(tag);
                if (data != null)
                {
                    UltimaTagLida = data.Tags.Find(x => x.TagId == tag);
                    if (UltimaTagLida != null)
                    {
                        LocalDefinido = true;
                        DadosEstabelecimento = data;
                        grafo = new GraphDFS(DadosEstabelecimento);
                        All_Lugares = DadosEstabelecimento.Lugares;
                        All_Lugares_Navegaveis = new List<LugaresTO>();
                        All_Itens = DadosEstabelecimento.Itens;
                        All_Itens_Navegaveis = new List<ItensTO>();
                        List<string> lugaresString = new List<string>();

                        foreach (var lugar in All_Lugares)
                        {
                            if (lugar.Navegavel)
                            {
                                All_Lugares_Navegaveis.Add(lugar);
                                lugaresString.Add(lugar.Nome);
                            }
                        }

                        foreach (var item in All_Itens)
                            if (item.Navegavel)
                                All_Itens_Navegaveis.Add(item);

                        STTHelper.RegistrarLugares(lugaresString);

                        if (!string.IsNullOrEmpty(data.NomeEstabelecimento)) 
                        _ = TTSHelper.Speak($"Bem vindo à {data.NomeEstabelecimento}");

                        DescreverTag();

                        return true;
                    }
                    
                }
            }
            catch (Exception err)
            {
            }

            _ = TTSHelper.Speak("Não consegui encontrar informações sobre o seu local!");
            return false;
        }

        private void DescreverTag()
        {
            if (UltimaTagLida != null && (UltimaTagLida != UltimaTagQueFoiDescrita ||
                (UltimaTagLida == UltimaTagQueFoiDescrita && swDescreverLugar.ElapsedMilliseconds > 30000)))
            {
                if (UltimaTagLida.tipoTag == (int)EnumTipoTag.lugar)
                {
                    swDescreverLugar.Restart();
                    UltimaTagQueFoiDescrita = UltimaTagLida;
                    var lugar = All_Lugares.Find(x => x.TAG_id == UltimaTagLida.Id);
                    if (lugar != null)
                        _ = TTSHelper.Speak($"Você está em: {lugar.Nome}!. {lugar.Descricao}");
                }
            }
        }
        private List<TagTO> ConverterRota(List<int> rota)
        {
            List<TagTO> retorno = new List<TagTO>();
            if (DadosEstabelecimento != null && rota !=null)
            {
                foreach (int id in rota)
                {
                    var aux = DadosEstabelecimento.Tags.Find(x => x.Id == id);
                    if (aux != null)
                        retorno.Add(aux);

                }

                if (retorno.Count != rota.Count)
                    return null;
                else
                    return retorno;
            
            }

            return null;
        }
        private EnumDirecao InverterDirecao(bool inverter, EnumDirecao direcao)
        {
            EnumDirecao retorno = direcao;

            if (!inverter)
                return direcao;

            switch(direcao)
            {
                case EnumDirecao.Direita:
                    return EnumDirecao.Esquerda;
                case EnumDirecao.Esquerda:
                    return EnumDirecao.Direita;
                case EnumDirecao.Frente:
                    return EnumDirecao.Tras;
                case EnumDirecao.Tras:
                    return EnumDirecao.Frente;

            }


            return retorno;

        }
        private string FalaDirecao(bool inverter, EnumDirecao direcao)
        {
            EnumDirecao direcaoCorrigida = InverterDirecao(inverter, direcao);

            switch (direcaoCorrigida)
            {
                case EnumDirecao.Direita:
                    return "Vire à direita";
                case EnumDirecao.Esquerda:
                    return "Vire à esquerda";
                case EnumDirecao.Frente:
                    return "Siga em frente";
                case EnumDirecao.Tras:
                    return "Vire para trás e continue";
            }

            return null;

        }
        private void DeterminarDirecao(TagTO atual, TagTO proxima,TagsPaiTO relacionamento)
        {
            string fala = FalaDirecao(atual.Id < proxima.Id, (EnumDirecao)relacionamento.Direcao);
            if (!string.IsNullOrEmpty(fala))
                _ = TTSHelper.Speak(fala);
        }
        private void DescreverDirecao()
        {
            if (RotaAtual != null)
            {
                int pos = 0;
                var tag = RotaAtual.Find(x => x.Id == UltimaTagLida.Id);
                pos = RotaAtual.FindIndex(x => x.Id == UltimaTagLida.Id);
                if (pos >= 0)
                {
                    if (pos == RotaAtual.Count - 1)
                    {
                        _ = TTSHelper.Speak("Você chegou ao seu destino!");
                        DescreverTag();
                        RotaAtual.Clear();
                        _lugarDesejado = null;
                        RotaAtual = null;
                    }
                    else
                    {
                        var proximaTag = RotaAtual[pos + 1];
                        var relacionamento = tag.TagsPai.Find(x => x.Id_Tag == proximaTag.Id || x.Id_Tag_Pai == proximaTag.Id);
                        if (relacionamento != null)
                            DeterminarDirecao(tag, proximaTag, relacionamento);
                    }


                }
                else
                {
                    _ = TTSHelper.Speak("Ops! vou recalcular a rota!");
                    RotaAtual.Clear();
                    RotaAtual = null;
                    CalcularRota(_lugarDesejado);
                }
                //TODO RECALCULANDO
            }
        }

        public async void CalcularRota(string lugarDesejado)
        {
            try
            {
                var Lugar = All_Lugares_Navegaveis.Find(x => x.Nome == lugarDesejado);
                if (Lugar != null)
                {
                    var tag = DadosEstabelecimento.Tags.Find(x => x.Id == Lugar.TAG_id);
                    if (tag != null)
                    {

                        List<TagTO> rota = ConverterRota(grafo.CalcularRota(UltimaTagLida.Id, tag.Id));
                        if (rota != null)
                        {
                            await TTSHelper.Speak("Rota calculada com sucesso!");                          
                            RotaAtual = rota;
                            _lugarDesejado = lugarDesejado;
                            PosAtualRota = 0;
                            DescreverDirecao();

                        }

                    }

                }
            }
            catch (Exception err)
            { }
        }

        public void SetLocal(string tag)
        {
            if (!LocalDefinido)
            {
                _ = TTSHelper.Speak("Por favor aguarde! estou verificando dados do seu local!");
                GetDadosFromServer(tag);
            }
            else if (DadosEstabelecimento != null)
            {
                var aux = DadosEstabelecimento.Tags.Find(x => x.TagId == tag);
                if (aux != null)
                {
                    UltimaTagLida = aux;

                    if (UltimaTagLida != UltimaTagQueFoiDescrita)
                    {
                        if (RotaAtual != null)
                            DescreverDirecao();

                        DescreverTag();
                    }

                }
                else
                {
                   /* //tag não encontrada... tenta encontrar no servidor uma nova relação
                    if (GetDadosFromServer(tag))
                    {

                    }*/
                }
            }

        }
    }
}
