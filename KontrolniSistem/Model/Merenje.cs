using KontrolniSistem.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolniSistem.Model
{
    public class Merenje : BindableBase
    {
        #region POLJA KLASE Merenje
        private int izmereno;

        private bool vanOpsega;
        #endregion

        #region PRAZAN KONSTURKTOR
        public Merenje()
        {
            // Prazan konstruktor
        }
        #endregion

        #region PROPERTY KLASE Merenje
        public int Izmereno
        {
            get
            {
                return izmereno;
            }
            set
            {
                if (izmereno != value)
                {
                    izmereno = value;
                    OnPropertyChanged("Izmereno");
                }

                if (izmereno < 45 || izmereno > 75)
                {
                    vanOpsega = true;
                    OnPropertyChanged("VanOpsega");
                }
                else
                {
                    vanOpsega = false;
                    OnPropertyChanged("VanOpsega");
                }
            }
        }

        public bool VanOpsega
        {
            get
            {
                return vanOpsega;
            }

            set
            {
                if (vanOpsega != value)
                {
                    vanOpsega = value;
                    OnPropertyChanged("VanOpsega");
                }
            }
        }
        #endregion
    }
}
