using System;
using System.Collections.Generic;
using System.Text;

namespace GuideMe.Gestos
{
    public class GestoSwipeCima:GestoSwipe
    {
        public GestoSwipeCima() : base()
        {
            _direcao = Enum.EnumSwipeDirection.Up;
        }

        public override string GetInfo()
        {
            return "swc";
        }
    }
}
