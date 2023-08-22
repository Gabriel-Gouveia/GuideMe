using GuideMe.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace GuideMe.Gestos
{
    public abstract class GestosBase
    {

        public GestosBase()
        {
            _Instante = DateTime.Now;
        }   

        protected EnumTipoGestos _tipoGesto;
        public EnumTipoGestos TipoGesto
        {
            get { return _tipoGesto; }
        }
        protected DateTime _Instante;
        public DateTime Instantes
        {
            get { return _Instante; }
        }

        public abstract string GetInfo();   

    }
}
