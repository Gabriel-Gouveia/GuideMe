using System;
using System.Collections.Generic;
using System.Text;

namespace GuideMe.TOs
{
    public class ResultCallApi
    {

        /// <summary>
        /// Retorno no formato string. quando houver
        /// </summary>
        public string Retorno { get; set; }

        /// <summary>
        /// Retorno no formato objeto, quando houver
        /// </summary>
        public object RetornoObj { get; set; }

        public Exception Erro { get; set; }


        public bool Sucesso { get; set; }



        public string MsgErro { get; set; }





    }
}
