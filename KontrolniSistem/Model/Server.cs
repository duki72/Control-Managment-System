using MVVMLight.Messaging;
using KontrolniSistem.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace KontrolniSistem.Model
{
    public class Server : ValidationBase
    {
        #region POLJA KLASE Entitet
        private int id;
        private string naziv;
        private string ip;
        private string slika;
        private int zauzece;
        private bool boja;
        private string klasa;
        private int canvas_pozicija;
        #endregion

        #region KONSTRUKTOR KLASE Entitet
        public Server()
        {
            // prazan konstruktor
        }

        public Server(int id, string naziv, string iP, string slika, int zauzece, bool boja, string klasa, int canvas_pozicija)
        {
            Id = id;
            Naziv = naziv;
            IP = iP;
            Slika = slika;
            Zauzece = zauzece;
            Boja = boja;
            Klasa = klasa;
            Canvas_pozicija = canvas_pozicija;
        }

        #endregion

        #region PROPERTY KLASE Entitet
        public int Id
        {
            get
            {
                return id;
            }

            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged("Id");
                }
            }
        }

        public string Naziv
        {
            get
            {
                return naziv;
            }

            set
            {
                if (naziv != value)
                {
                    naziv = value;
                    OnPropertyChanged("Naziv");
                }
            }
        }

        public string IP
        {
            get
            {
                return ip;
            }

            set
            {
                if (ip != value)
                {
                    ip = value;
                    OnPropertyChanged("IP");
                }
            }
        }

        public string Slika
        {
            get
            {
                return slika;
            }

            set
            {
                if (slika != value)
                {
                    slika = value;
                    OnPropertyChanged("Slika");
                }
            }
        }

        public int Zauzece
        {
            get
            {
                return zauzece;
            }

            set
            {
                if (zauzece != value)
                {
                    zauzece = value;
                    OnPropertyChanged("Zauzece");
                }

                if (zauzece < 45 || zauzece > 75)
                {
                    Boja = true;
                    Slika = "/Assets/deviceerror.png";

                    // samo ako je na canvasu ispisuje se poruka
                    if (Canvas_pozicija != -1)
                    {
                        DataMessenger message = new DataMessenger()
                        {
                            Visibility_Uspesno = Visibility.Hidden,
                            Visibility_Greska = Visibility.Visible,
                            Poruka = "⚠ Entitet (" + IP + ", " + Naziv + ", " + IP + ") je prijavio KRITIČNU VREDNOST " + Zauzece + "%!"
                        };

                        Messenger.Default.Send(message);
                    }
                }
                else
                {
                    Boja = false;
                    Slika = "/Assets/device.png";

                    // samo ako je na canvasu ispisuje se poruka
                    if (Canvas_pozicija != -1)
                    {
                        DataMessenger message = new DataMessenger()
                        {
                            Visibility_Uspesno = Visibility.Visible,
                            Visibility_Greska = Visibility.Hidden,
                            Poruka = " Entitet (" + IP + ", " + Naziv + ", " + IP + ") je prijavio REGULARNU VREDNOST " + Zauzece + "%."
                        };

                        Messenger.Default.Send(message);
                    }
                }

                OnPropertyChanged("Boja");
                OnPropertyChanged("Slika");
            }
        }

        public bool Boja
        {
            get
            {
                return boja;
            }

            set
            {
                if (boja != value)
                {
                    boja = value;
                    OnPropertyChanged("Boja");
                }
            }
        }

        public string Klasa
        {
            get
            {
                return klasa;
            }

            set
            {
                if (klasa != value)
                {
                    klasa = value;
                    OnPropertyChanged("Klasa");
                }
            }
        }

        public int Canvas_pozicija
        {
            get
            {
                return canvas_pozicija;
            }

            set
            {
                if (canvas_pozicija != value)
                {
                    canvas_pozicija = value;
                    OnPropertyChanged("Canvas_pozicija");
                }
            }
        }
        #endregion

        #region METODA ZA MODELOVANI ISPIS ENTITETA
        public override string ToString()
        {
            return "Entitet " + Id + ": " + Naziv + " (" + IP + ")";
        }

        protected override void ValidateSelf()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
