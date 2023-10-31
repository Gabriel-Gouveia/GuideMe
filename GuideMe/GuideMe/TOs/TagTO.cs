using System;
using System.Collections.Generic;
using System.Text;

namespace GuideMe.TOs
{
    public class TagTO
    {
        public int Id { get; set; }
        public string TagId { get; set; }
        public int EstabelecimentoId { get; set; }
        public int tipoTag { get; set; }
        public string Nome { get; set; }
        public List<TagsPaiTO> TagsPai { get; set; }
    }
}
