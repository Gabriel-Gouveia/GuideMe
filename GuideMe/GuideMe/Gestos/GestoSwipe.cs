using GuideMe.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace GuideMe.Gestos
{
    public abstract class GestoSwipe:GestosBase
    {
        protected EnumSwipeDirection _direcao;
        public EnumSwipeDirection Direcao
        {
            get { return _direcao; }
        }
        public GestoSwipe():base()
        {
            _tipoGesto=Enum.EnumTipoGestos.Swipe;
        }
    }
}
