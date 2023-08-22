using System;
using System.Collections.Generic;
using System.Text;

namespace GuideMe.Gestos
{
    public class GestoSwipeDireita:GestoSwipe
    {
        public GestoSwipeDireita():base()
        {
            _direcao=Enum.EnumSwipeDirection.Right;
        }

        public override string GetInfo()
        {
            return "swd";
        }
    }
}
