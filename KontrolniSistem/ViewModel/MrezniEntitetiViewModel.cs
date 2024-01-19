using MVVMLight.Messaging;
using KontrolniSistem.Helpers;
using KontrolniSistem.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace KontrolniSistem.ViewModel
{
    public class MrezniEntitetiViewModel : BindableBase
    {
        private static readonly ObservableCollection<string> klaseServera = new ObservableCollection<string>
                                                                   { "Klasa Servera 1", "Klasa Servera 2", "Klasa Servera 3",
                                                                     "Klasa Servera 4", "Klasa Servera 5"};
        //komanda za filtriranje
        public MyICommand FiltrirajKomanda { get; set; }

        // Komanda za dodavanje
        public MyICommand DodajKomanda { get; set; }

        // Komanda za brisanje
        public MyICommand ObrisiKomanda { get; set; }

        //polja
        private enum KLASE { Server_1, Server_2, Server_3, Server_4, Server_5 };

        private int odabranaKlasaIndeks;

        private bool izabranNaziv;
        private bool izabranTip;

        private string trenutnaVrednost;

        public static ObservableCollection<Filter> IstorijaFiltera { get; set; }

        private int odabraniIndeksIstorijeFiltera;

        private int odabraniIndeksDodavanjeEntiteta;

        private bool moguceBrisanje;

        private Server odabraniEntitet;

        private Filter odabraniFilter = new Filter();

        private Visibility uspesno, greska, informacija;
        private string poruka;

        public static ObservableCollection<Server> listaServera { get; set; }


        //konstruktor klase 

        public MrezniEntitetiViewModel()
        {
            odabranaKlasaIndeks = 0;
            izabranNaziv = false;
            izabranTip = true;
            trenutnaVrednost = null;
            IstorijaFiltera = new ObservableCollection<Filter>();
            OdabraniIndeksDodavanjeEntiteta = 0;
            OdabraniIndeksIstorijeFiltera = 0;
            FiltrirajKomanda = new MyICommand(OnFilterPress);
            ObrisiKomanda = new MyICommand(OnBrisanjePress);
            DodajKomanda = new MyICommand(OnDodajPress);
            listaServera = MainWindowViewModel.Serveri; //prikaz svih entiteta na pocetku
            MoguceBrisanje = false;
            OdabraniEntitet = null; // nije odabran nijedan entitet
            Uspesno = Greska = Informacija = Visibility.Hidden;
        }

        

        //property

        public Server OdabraniEntitet
        {
            get
            {
                return odabraniEntitet;
            }

            set
            {
                if (odabraniEntitet != value)
                {
                    odabraniEntitet = value;
                    OnPropertyChanged("OdabraniEntitet");
                    OnPropertyChanged("MoguceBrisanje");
                }
            }
        }

        public bool MoguceBrisanje
        {
            get
            {
                return OdabraniEntitet != null;
            }

            set
            {
                if (moguceBrisanje != value)
                {
                    moguceBrisanje = value;
                    OnPropertyChanged("MoguceBrisanje");
                    OnPropertyChanged("Background");
                }
            }
        }

        public int OdabranaKlasaIndeks
        {
            get
            {
                return odabranaKlasaIndeks;
            }

            set
            {
                if (odabranaKlasaIndeks != value)
                {
                    odabranaKlasaIndeks = value;
                    OnPropertyChanged("OdabranaKlasaIndeks");
                }
            }
        }
        public int OdabraniIndeksDodavanjeEntiteta
        {
            get
            {
                return odabraniIndeksDodavanjeEntiteta;
            }

            set
            {
                if (odabraniIndeksDodavanjeEntiteta != value)
                {
                    odabraniIndeksDodavanjeEntiteta = value;
                    OnPropertyChanged("OdabraniIndeksDodavanjeEntiteta");
                }
            }
        }

        public int OdabraniIndeksIstorijeFiltera
        {
            get
            {
                return odabraniIndeksIstorijeFiltera;
            }

            set
            {
                if (odabraniIndeksIstorijeFiltera != value)
                {
                    odabraniIndeksIstorijeFiltera = value;
                    OnPropertyChanged("OdabraniIndeksIstorijeFiltera");
                }
            }
        }


        //propertiji za radio button 
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

                    if (izabranNaziv)
                    {
                        izabranTip = false;
                        

                        OnPropertyChanged("IzabranTip");
                     
                    }

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
                    izabranTip= value;

                    if (izabranTip)
                    {
                        izabranNaziv = false;


                        OnPropertyChanged("IzabranNaziv");

                    }

                    OnPropertyChanged("IzabranTip");
                }
            }
        }

        //properti za trenutno uneti Id

        public string TrenutnaVrednost
        {
            get
            {
                return trenutnaVrednost;
            }
            set
            {
                if(trenutnaVrednost != value) 
                { 
                    trenutnaVrednost = value;
                    OnPropertyChanged("TrenutnaVrednost");
                }
            }
        }

        //properti za trenutno odabrani filter

        public Filter OdabraniFilter
        {
            get
            {
                return odabraniFilter;
            }
            set
            {
                if (odabraniFilter != value)
                {
                    odabraniFilter = value;

                    OdabranaKlasaIndeks = odabraniFilter.IndeksUListiKlaseServera;
                    IzabranNaziv = odabraniFilter.IzabranNaziv;
                    IzabranTip = odabraniFilter.IzabranTip;
                    TrenutnaVrednost = odabraniFilter.TrazenaVrednost;

                    OnPropertyChanged("OdabraniFilter");
                    OnPropertyChanged("OdabranaKlasaIndeks");
                    OnPropertyChanged("IzabranNaziv");
                    OnPropertyChanged("IzabranTip");
                    OnPropertyChanged("TrenutnaVrednost");

                    //primeni filter
                    OnFilterPress();
                }
            }
        }


        //properti za poruku

        public string Poruka
        {
            get
            {
                return poruka;
            }

            set
            {
                if (poruka != value)
                {
                    poruka = value;
                    OnPropertyChanged("Poruka");
                }
            }
        }
        public Visibility Uspesno
        {
            get
            {
                return uspesno;
            }

            set
            {
                if (uspesno != value)
                {
                    uspesno = value;

                    if (uspesno == Visibility.Visible)
                    {
                        Greska = Informacija = Visibility.Hidden;
                        OnPropertyChanged("Greska");
                        OnPropertyChanged("Informacija");
                    }

                    OnPropertyChanged("Uspesno");
                }
            }
        }

        public Visibility Greska
        {
            get
            {
                return greska;
            }

            set
            {
                if (greska != value)
                {
                    greska = value;

                    if (greska == Visibility.Visible)
                    {
                        uspesno = informacija = Visibility.Hidden;
                        OnPropertyChanged("Uspesno");
                        OnPropertyChanged("Informacija");
                    }

                    OnPropertyChanged("Greska");
                }
            }
        }

        public Visibility Informacija
        {
            get
            {
                return informacija;
            }

            set
            {
                if (informacija != value)
                {
                    informacija = value;

                    if (informacija == Visibility.Visible)
                    {
                        greska = uspesno = Visibility.Hidden;
                        OnPropertyChanged("Greska");
                        OnPropertyChanged("Uspesno");
                    }

                    OnPropertyChanged("Informacija");
                }
            }
        }
        public static ObservableCollection<string> KlaseServera
        {
            get
            {
                return klaseServera;
            }
        }

        public ObservableCollection<Server> ListaServera
        {
            get
            {
                return listaServera;
            }

            set
            {
                if (listaServera != null)
                {
                    listaServera= value;
                    OnPropertyChanged("ListaServera");
                }
            }
        }

        //metode za dodavanje, brisanje, filtriranje
        #region Metode za dodavanje i brisanje, filtriranje
        public void OnDodajPress()
        {
            OdabraniEntitet = null;
            ListaServera = MainWindowViewModel.Serveri;

            //na osnovu odabrane klase servera kreira random objekat

            //novi id je trenutni najveci id + 1
            int max_id = ListaServera.Count != 0 ? listaServera.Max(x => x.Id) + 1 : 1;

            int odabrana_klasa_servera = OdabraniIndeksDodavanjeEntiteta;


            const int ip_min = 0, ip_max = 255;
            int ip_prvi_oktet, ip_drugi_oktet, ip_treci_oktet, ip_cetvrti_oktet;
            string ip, klasa;

            // generisanje na osnovu odabrane adresne klase
            if (odabrana_klasa_servera == 0)
            {
                ip_prvi_oktet = new Random().Next(1, 127);
                klasa = "Server_1";
            }
            else if (odabrana_klasa_servera == 1)
            {
                ip_prvi_oktet = new Random().Next(128, 191);
                klasa = "Server_2";
            }
            else if (odabrana_klasa_servera == 2)
            {
                ip_prvi_oktet = new Random().Next(192, 223);
                klasa = "Server_3";
            }
            else if (odabrana_klasa_servera == 3)
            {
                ip_prvi_oktet = new Random().Next(224, 239);
                klasa = "Server_4";
            }
            else if (odabrana_klasa_servera == 4)
            {
                ip_prvi_oktet = new Random().Next(240, 255);
                klasa = "Server_5";
            }
            else
            {
                ip_prvi_oktet = 0;
                klasa = "Server_1";
            }

            ip_drugi_oktet = new Random().Next(ip_min, ip_max);
            Thread.Sleep(50);
            ip_treci_oktet = new Random().Next(ip_min, ip_max);
            Thread.Sleep(50);
            ip_cetvrti_oktet = new Random().Next(ip_min, ip_max);

            ip = ip_prvi_oktet + "." + ip_drugi_oktet + "." + ip_treci_oktet + "." + ip_cetvrti_oktet;
            string naziv = "Server_" + (max_id < 10 ? ("0" + max_id).ToString() : max_id.ToString());

            Messenger.Default.Send(
                    new Server() { Id = max_id, Naziv = naziv, IP= ip, Slika = "/Assets/device.png",Zauzece = new Random().Next(0,100), 
                        Klasa = klasa, Canvas_pozicija = -1 });

            //poruka
            Uspesno = Visibility.Visible;
            Poruka = " Novi entitet (" + max_id + ", " + naziv + ", " + ip + ") je uspešno dodat u sistem!";

        }

        private void OnBrisanjePress()
        {
            int id = OdabraniEntitet.Id;
            string naziv = OdabraniEntitet.Naziv;
            string ip = OdabraniEntitet.IP;

            Messenger.Default.Send(ListaServera.IndexOf(OdabraniEntitet));
            OdabraniEntitet = null;
            Messenger.Default.Send(listaServera);

            Greska = Visibility.Visible;
            Poruka = " Entitet (" + id + ", " + naziv + ", " + ip + ") je uspešno izbrisan iz sistema!";
        }

        private void OnFilterPress()
        {
            Filter istorija = new Filter
            {
                IndeksUListiKlaseServera = OdabranaKlasaIndeks,
                IzabranNaziv = IzabranNaziv,
                IzabranTip = IzabranTip,
                TrazenaVrednost = TrenutnaVrednost
            };

            //ako filter vec postoji u listi filtera - ne dodaje se
            bool postoji = false;
            foreach(Filter filter in IstorijaFiltera)
            {
                if (filter.Equals(istorija))
                {
                    postoji = true;
                    break;
                }
            }

            //ako ne postoji dodaje se u istoriju filtera

            if (!postoji)
            {
                IstorijaFiltera.Add(istorija);
            }

            //zatim primena filtera
            ListaServera = Filtriranje();

           

            Informacija = Visibility.Visible;
            Poruka = "Filter (" + istorija + ") je uspešno primenjen! Filtirarani entiteti su vidljivi u tabeli!";

            //Thread.Sleep(7);

            //Messenger.Default.Send(listaServera);

        }

        ObservableCollection<Server> Filtriranje()
        {
            ObservableCollection<Server> filtrirani = new ObservableCollection<Server>();
            ObservableCollection<Server> svi_serveri = MainWindowViewModel.Serveri;

            OdabraniEntitet = null;

            if(svi_serveri != null && svi_serveri.Count > 0)
            {
                foreach(Server s in svi_serveri)
                {
                    
                        if (PrimeniFilter(s))
                        {
                            filtrirani.Add(s);  
                        }
                    
                }
            }

            //if(filtrirani.Count != 0)
            //{
            //    MainWindowViewModel.Serveri = filtrirani;
            //}

            return filtrirani;
        }
        #endregion

        #region FILTRIRANJE PODATAKA

        

        //filtriranje podataka
        bool PrimeniFilter(Server trenutni)
        {
            if (IzabranNaziv && TrenutnaVrednost == trenutni.Naziv)
            {
                IzabranTip = false;
                return true;
            }
            else if (IzabranTip && IzabranNaziv && TrenutnaVrednost == trenutni.Klasa)
            {
                IzabranNaziv = false;
                return true;
            }

            else
            {
                return false;
            }
        }

        #endregion
    }
}
