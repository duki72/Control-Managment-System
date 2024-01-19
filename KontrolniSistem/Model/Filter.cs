using KontrolniSistem.Helpers;
using KontrolniSistem.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolniSistem.Model
{
    public class Filter :BindableBase
    {
        private int indeksUListiKlaseServera;
        private bool izabranNaziv;
        private bool izabranTip;
       
        private string trazenaVrednost;

        public Filter()
        {

        }

        public int IndeksUListiKlaseServera
        {
            get
            {
                return indeksUListiKlaseServera;
            }

            set
            {
                if (indeksUListiKlaseServera != value)
                {
                    indeksUListiKlaseServera = value;
                    OnPropertyChanged("IndeksUListiKlaseServera");
                }
            }
        }

        public bool IzabranNaziv
        {
            get
            {
                return izabranNaziv;
            }
            set
            {
                if (izabranNaziv != value)
                {
                    izabranNaziv = value;
                    OnPropertyChanged("IzabranNaziv");
                }
            }
        }

        public bool IzabranTip
        {
            get
            {
                return izabranTip;
            }
            set
            {
                if (izabranTip != value)
                {
                    izabranTip = value;
                    OnPropertyChanged("IzabranTip");
                }
            }
        }

        public string TrazenaVrednost
        {
            get
            {
                return trazenaVrednost;
            }

            set
            {
                if (trazenaVrednost != value)
                {
                    trazenaVrednost = value;
                    OnPropertyChanged("TrazenaVrednost");
                }
            }
        }

        public override bool Equals(object obj)
        {
            return obj is Filter filter &&
                   IndeksUListiKlaseServera == filter.IndeksUListiKlaseServera &&
                   IzabranNaziv == filter.IzabranNaziv &&
                   IzabranTip == filter.IzabranTip &&
                   TrazenaVrednost == filter.trazenaVrednost ;
                  
        }

        public override string ToString()
        {
            string formatirano = MrezniEntitetiViewModel.KlaseServera[IndeksUListiKlaseServera] + " | ";

            if (IzabranNaziv) formatirano += "Naziv: " + TrazenaVrednost;
            if (IzabranTip) formatirano += "Tip:" + TrazenaVrednost;
            

            return formatirano;
        }
    }
}
