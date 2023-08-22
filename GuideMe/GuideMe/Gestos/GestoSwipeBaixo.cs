using System;
using System.Collections.Generic;
using System.Text;

namespace GuideMe.Gestos
{
    public class GestoSwipeBaixo : GestoSwipe
    {
        public GestoSwipeBaixo() : base()
        {
            _direcao = Enum.EnumSwipeDirection.Down;
        }

        public override string GetInfo()
        {
            return "swb";
        }
    }
}
