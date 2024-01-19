using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolniSistem.Model
{
    public class KlasifikovaniEntiteti
    {
        public BindingList<Server> ListaEntiteta { get; set; }

        public string KlasaServera { get; set; }

        public KlasifikovaniEntiteti()
        {
            ListaEntiteta = new BindingList<Server>();
        }
    }
}
