using KontrolniSistem.Helpers;
using KontrolniSistem.Model;
using KontrolniSistem.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MVVMLight.Messaging;
using System.Windows.Shapes;

namespace KontrolniSistem.ViewModel
{
    public class MainWindowViewModel : BindableBase
    {
        private BindableBase currentViewModel;
        public MyICommand<Window> Izlaz { get; private set; }
        public MyICommand<string> NavCommand { get; private set; }

        public MrezniEntitetiViewModel mrezniEntitetiViewModel;
        public RasporedMrezeViewModel rasporedMreze;
        public StatistikaMrezeViewModel statistikaMreze;
        public static List<string> System_Delimiter = new List<string>();
        public PocetnaViewModel pocetnaViewModel;
       // public static string path = "pack://application:,,,/Assets/uredjaj.png";

        public static ObservableCollection<Server> Serveri { get; set; } = new ObservableCollection<Server>();

        private string poruka;
         

        
        public MainWindowViewModel()
        {


            createListener();  //povezivanje sa serverskom aplikacijom


            NavCommand = new MyICommand<string>(OnNav);
            Izlaz = new MyICommand<Window>(CloseWindow);


            Serveri = new ObservableCollection<Server>();

            pocetnaViewModel = new PocetnaViewModel();
            mrezniEntitetiViewModel = new MrezniEntitetiViewModel();
            rasporedMreze = new RasporedMrezeViewModel();
            CurrentViewModel = pocetnaViewModel;

            Messenger.Default.Register<Server>(this, DodajServer);
            Messenger.Default.Register<int>(this, UkloniServer);
            Messenger.Default.Register<ObservableCollection<Server>>(this, ListaSvihServera);


            //test podaci pri pokretanju aplikacije
            for (int i = 0; i < 3; i++)
            {
                mrezniEntitetiViewModel.OnDodajPress();
            }

            mrezniEntitetiViewModel.OdabraniIndeksDodavanjeEntiteta = new Random().Next(0, 4);

            for (int i = 0; i < 3; i++)
            {
                mrezniEntitetiViewModel.OnDodajPress();
            }

            mrezniEntitetiViewModel.OdabraniIndeksDodavanjeEntiteta = new Random().Next(0, 4);



            //mrezniEntitetiViewModel.OdabraniIndeksDodavanjeEntiteta = 0;

            statistikaMreze = new StatistikaMrezeViewModel();

            //brisem stari log.txt
            if (File.Exists("log.txt"))
            {
                File.Delete("log.txt");
            }


            statistikaMreze.AzuriranjeMerenja();

           
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

        //TCP Mrezna konekcija sa serverom
        private void createListener()
        {
            var tcp = new TcpListener(IPAddress.Any, 25565);
            tcp.Start();

            var listeningThread = new Thread(() =>
            {
                while (true)
                {
                    var tcpClient = tcp.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(param =>
                    {
                        //Prijem poruke
                        NetworkStream stream = tcpClient.GetStream();
                        string incomming;
                        byte[] bytes = new byte[1024];
                        int i = stream.Read(bytes, 0, bytes.Length);
                        //Primljena poruka je sacuvana u incomming stringu
                        incomming = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                        //Ukoliko je primljena poruka pitanje koliko objekata ima u sistemu -> odgovor
                        if (incomming.Equals("Need object count"))
                        {
                            //Response
                            /* Umesto sto se ovde salje count.ToString(), potrebno je poslati 
                             * duzinu liste koja sadrzi sve objekte pod monitoringom, odnosno
                             * njihov ukupan broj (NE BROJATI OD NULE, VEC POSLATI UKUPAN BROJ)
                             * */
                            Byte[] data = Encoding.ASCII.GetBytes(Serveri.Count.ToString());
                            stream.Write(data, 0, data.Length);
                        }
                        else
                        {
                            //U suprotnom, server je poslao promenu stanja nekog objekta u sistemu
                            // Console.WriteLine(incomming); //Na primer: "Entitet_1:272"

                            //################ IMPLEMENTACIJA ####################
                            // Obraditi poruku kako bi se dobile informacije o izmeni
                            // Azuriranje potrebnih stvari u aplikaciji
                            int len = incomming.Length;
                            string substring = incomming.Substring(8, len - 8);
                            string[] splitovano = substring.Split(':');
                            int id = int.Parse(splitovano[0]); // id entiteta koji se menja
                            int zauzece = int.Parse(splitovano[1]); // zauzece koje se menja

                            Server za_izmenu = Serveri.FirstOrDefault(p => p.Id == id + 1);

                            //ako je obrisan objekat a simulator se jos nije restartovao - odbaciti
                            if(za_izmenu != null)
                            {
                               int staro = za_izmenu.Zauzece;
                                za_izmenu.Zauzece = zauzece;

                                //ispis poruke
                                mrezniEntitetiViewModel.Informacija = Visibility.Visible;
                                mrezniEntitetiViewModel.Poruka = "Entitet (" + za_izmenu.IP + ", " + za_izmenu.Naziv + ", " + za_izmenu.IP + ") je prijavio novu vrednost u sistemu " + staro + " -> " + zauzece + "!";
                            }


                            //upis u log.txt
                            string za_upis = ((id + 1) + "-" + zauzece).ToString() + "-" + DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                            File.AppendAllText("log.txt", za_upis + "\n");
                            Delimit_File(za_upis);

                        }
                    }, null);

                }

            });

            listeningThread.IsBackground =true;
            listeningThread.Start();
            
        }

        //navigacija
        public BindableBase CurrentViewModel
        {
            get { return currentViewModel; }
            set
            {
                SetProperty(ref currentViewModel, value);
            }
        }
        private void OnNav(string destinacija)
        {
            switch (destinacija)
            {
                case "pocetna":
                    CurrentViewModel = pocetnaViewModel;
                    break;
                case "mrezni_entiteti":
                    CurrentViewModel = mrezniEntitetiViewModel;
                    break;
                case "raspored_mreze":
                    CurrentViewModel = rasporedMreze;
                    break;
                case "statistika_mreze":
                    CurrentViewModel = statistikaMreze;
                    break;

            }
        }

        private void DodajServer(Server novi)
        {
            
            Serveri.Add(novi);
            Messenger.Default.Send(new ServerTreeView() { Server = novi });
        }

        private void UkloniServer(int index)
        {
            int indeks = Serveri[index].Canvas_pozicija;
            Serveri.RemoveAt(index);

            if (indeks != -1)
            {
                Messenger.Default.Send(indeks);
            }
        }

        private void ListaSvihServera(ObservableCollection<Server> servers)
        {
            servers = Serveri;
        }

        void Delimit_File(string str)
        {
            System_Delimiter.Add(str);
            int old_id = statistikaMreze.OdabraniId;
            statistikaMreze.OdabraniId = 1;
            statistikaMreze.OdabraniId = old_id;
        }
        private void CloseWindow(Window obj)
        {
            obj.Close();
        }

    }
      
}

