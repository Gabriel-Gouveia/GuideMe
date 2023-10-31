using System;
using System.Collections.Generic;
using System.Text;

namespace GuideMe.TOs
{
    public class EstabelecimentoTagsTO
    {
        public List<TagTO> Tags { get; set; }
        public List<LugaresTO> Lugares { get; set; }
        public List<ItensTO> Itens { get; set; }
    }
}
