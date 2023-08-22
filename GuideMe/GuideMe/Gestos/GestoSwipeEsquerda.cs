using System;
using System.Collections.Generic;
using System.Text;

namespace GuideMe.Gestos
{
    public class GestoSwipeEsquerda:GestoSwipe
    {
        public GestoSwipeEsquerda() : base()
        {
            _direcao = Enum.EnumSwipeDirection.Left;
        }

        public override string GetInfo()
        {
            return "swe";
        }
    }
    
}
