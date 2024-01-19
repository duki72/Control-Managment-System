using KontrolniSistem.Helpers;
using KontrolniSistem.Model;
using KontrolniSistem.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using MVVMLight.Messaging;

namespace KontrolniSistem.ViewModel
{
    public class RasporedMrezeViewModel : BindableBase
    {

        //polja

        public static BindingList<KlasifikovaniEntiteti> EntitetiCanvas { get; set; }

        public static BindingList<KlasifikovaniEntiteti> EntitetiTreeView { get; set; } 


        //komande za drag && drop

        public MyICommand<Canvas> DragOverKomanda { get; private set; }

        public MyICommand<Canvas> DropKomanda { get; private set; }
        public MyICommand MouseLevoDugme { get; private set; }
        public MyICommand<TreeView> TreeViewOdabran { get; private set; }
        public MyICommand<Canvas> OslobodiKomanda { get; private set; }
        public MyICommand<Grid> NasumicnoRasporedi { get; private set; }


        //komande za drag && drop sa canvasa na canvas

        public MyICommand<Canvas> PreviewMouseUpKomanda { get; private set; }
        public MyICommand<Canvas> PreviewMouseMoveKomanda { get; private set; }
        public MyICommand<Canvas> PreviewMouseDownKomanda { get; private set; }

        //polja za trenutni entitet D&&D

        private Server draggedItem = null;
        private bool dragging = false;
        private int selected;
        private Canvas pocetni = null;

        private Visibility uspesno, greska, informacija;
        public static Grid desni;

       
        private string poruka;

        #region Properti za poruke
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

        #endregion

        //Inicijalni konstruktor
        public RasporedMrezeViewModel()
        {


           NasumicnoRasporedi = new MyICommand<Grid>(Rasporedi);

            // komande
            DragOverKomanda = new MyICommand<Canvas>(DragOverMetoda);
            DropKomanda = new MyICommand<Canvas>(DropMetoda);
            MouseLevoDugme = new MyICommand(TreeView_MouseLeftButtonUp);
            TreeViewOdabran = new MyICommand<TreeView>(Promena_SelectedItemChanged);
            OslobodiKomanda = new MyICommand<Canvas>(Oslobodi_Dugme);
           

            // komande za d&d
            PreviewMouseUpKomanda = new MyICommand<Canvas>(PreviewMouseUp);
            PreviewMouseMoveKomanda = new MyICommand<Canvas>(PreviewMouseMove);
            PreviewMouseDownKomanda = new MyICommand<Canvas>(PreviewMouseDown);

           

            //liste za entitete za tree view i canvas
            InicijalizacijaListi();

            //za prijem novih entiteta
            Messenger.Default.Register<ServerTreeView>(this, DodajuTreeViewListu);

            Messenger.Default.Register<DataMessenger>(this, Obavjestenje);

            //za uklanjanje entiteta ako se ukloni iz liste svih
            Messenger.Default.Register<ObrisiServer>(this, UkloniElement);

        }

        //notifikacija na views
        private void Obavjestenje(DataMessenger message)
        {
            Greska = message.Visibility_Greska;
            Uspesno = message.Visibility_Uspesno;
            Poruka = message.Poruka;
        }
       

        //kako bi se pamtilo stanje na canvasu  potrebno je koristiti konstruktor sa parametrima

        public RasporedMrezeViewModel(Grid leviGridCanvas)
        {

            desni = leviGridCanvas;

            NasumicnoRasporedi = new MyICommand<Grid>(Rasporedi);

            // komande
            DragOverKomanda = new MyICommand<Canvas>(DragOverMetoda);
            DropKomanda = new MyICommand<Canvas>(DropMetoda);
            MouseLevoDugme = new MyICommand(TreeView_MouseLeftButtonUp);
            TreeViewOdabran = new MyICommand<TreeView>(Promena_SelectedItemChanged);
            OslobodiKomanda = new MyICommand<Canvas>(Oslobodi_Dugme);

            // komande za d&d
            PreviewMouseUpKomanda = new MyICommand<Canvas>(PreviewMouseUp);
            PreviewMouseMoveKomanda = new MyICommand<Canvas>(PreviewMouseMove);
            PreviewMouseDownKomanda = new MyICommand<Canvas>(PreviewMouseDown);


            // indeksiranje
            // dock paneli krecu od indeksa 1
            // indeks 1 u dock panelu je canvas
            List<Canvas> kanvasi = new List<Canvas>();

            for (int i = 1; i < 13; i++)
            {
                DockPanel panel = (DockPanel)(leviGridCanvas.Children[i]);
                Canvas canvas = (Canvas)(panel.Children[1]);
                kanvasi.Add(canvas);
            }

            foreach (KlasifikovaniEntiteti ke in EntitetiCanvas.ToList())
            {
                foreach (Server s in ke.ListaEntiteta.ToList())
                {
                    if (s.Canvas_pozicija != -1)
                    {
                        draggedItem = MainWindowViewModel.Serveri.FirstOrDefault(p => p.Id == s.Id);
                        Canvas kanvas = kanvasi[s.Canvas_pozicija - 1];

                        TextBlock ispis = ((TextBlock)(kanvas).Children[0]);

                        if (draggedItem != null)
                        {
                            if (kanvas.Resources["taken"] == null)
                            {
                                BitmapImage img = new BitmapImage();
                                img.BeginInit();
                                string putanja = "pack://application:,,,/Assets/uredjaj.png";
                                img.UriSource = new Uri(putanja, UriKind.Absolute);
                                img.EndInit();
                                kanvas.Background = new ImageBrush(img);
                                ispis.Text = draggedItem.Naziv;
                                ispis.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#000000"));
                                draggedItem.Canvas_pozicija = GetCanvasId(kanvas.Name);
                                kanvas.Resources.Add("taken", true);
                            }
                            draggedItem = null;
                            dragging = false;
                        }
                    }
                }

            }

           Messenger.Default.Register<int>(this, UkloniAkoJeNaCanvasu);
        }

        private void UkloniAkoJeNaCanvasu(int id_kanvasa)
        {
            //ako nije inicijalizovan view onda return
            if (desni == null)
                return;

            List<Canvas> kanvasi = new List<Canvas>();

            for (int i = 1; i < 13; i++)
            {
                DockPanel panel = (DockPanel)desni.Children[i];
                Canvas canvas = (Canvas)(panel.Children[1]);
                kanvasi.Add(canvas);
            }

            //poziva se oslobodi dugme
            Oslobodi_Dugme(kanvasi[id_kanvasa]);
        }

        private void UkloniElement(ObrisiServer obj)
        {
            Server za_brisanje = obj.Server;
            int klasa = 0;

            if (za_brisanje.Klasa.Equals("Server_1")) klasa = 0;
            if (za_brisanje.Klasa.Equals("Server_2")) klasa = 1;
            if (za_brisanje.Klasa.Equals("Server_3")) klasa = 2;
            if (za_brisanje.Klasa.Equals("Server_4")) klasa = 3;
            if (za_brisanje.Klasa.Equals("Server_5")) klasa = 4;

            //ako se elemwnt nalazi na canvasu, ukloni ga iz liste i sa canvasa
            if (EntitetiCanvas[klasa].ListaEntiteta.Contains(za_brisanje))
            {
                EntitetiCanvas[klasa].ListaEntiteta.Remove(za_brisanje);


            }

            if (EntitetiTreeView[klasa].ListaEntiteta.Contains(za_brisanje))
            {
                EntitetiTreeView[klasa].ListaEntiteta.Remove(za_brisanje);


            }
        }

        private void DodajuTreeViewListu(ServerTreeView obj)
        {
            Server novi = obj.Server;
            int klasa = 0;

            if (novi.Klasa.Equals("Server_1")) klasa = 0;
            if (novi.Klasa.Equals("Server_2")) klasa = 1;
            if (novi.Klasa.Equals("Server_3")) klasa = 2;
            if (novi.Klasa.Equals("Server_4")) klasa = 3;
            if (novi.Klasa.Equals("Server_5")) klasa = 4;

            EntitetiTreeView[klasa].ListaEntiteta.Add(novi);


        }


        private void PreviewMouseDown(Canvas roditelj)
        {
            //prvo pronadjemo koji je to element na canvasu
            string naziv_entiteta = ((TextBlock)roditelj.Children[0]).Text;
            Server nadjen = null;

            foreach (KlasifikovaniEntiteti ke in EntitetiCanvas)
            {
                foreach(Server s in ke.ListaEntiteta)
                {
                    if (s.Naziv.Equals(naziv_entiteta))
                    {
                        nadjen = s;
                        break;
                    }
                }
            }

            draggedItem = nadjen;
            pocetni = roditelj;

        }

        private void PreviewMouseMove(Canvas obj)
        {
            if (draggedItem == null)
                return;
        }

        private void PreviewMouseUp(Canvas canvas)
        {
           
            //ako imam pocetni canvas i odabrani element i canvas na koji prebacujem nije vec zauzet

            if(draggedItem != null && pocetni != null && canvas.Resources["taken"] == null) {
                    
                //prebacujem sa novi canvas
                TextBlock ispis = ((TextBlock)(canvas).Children[0]);

                if(draggedItem != null)
                {
                    if(canvas.Resources["taken"] == null)
                    {
                        BitmapImage img = new BitmapImage();
                        img.BeginInit();
                        string putanja = "pack://application:,,,/Assets/uredjaj.png";
                        img.UriSource = new Uri(putanja, UriKind.Absolute);
                        img.EndInit();
                        canvas.Background = new ImageBrush(img);
                        ispis.Text = draggedItem.Naziv;
                        ispis.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#000000"));
                        draggedItem.Canvas_pozicija = GetCanvasId(canvas.Name);
                        canvas.Resources.Add("taken", true);
                    }

                    draggedItem= null;
                    dragging = false;
                }

                if (pocetni.Resources["taken"] != null)
                {
                    pocetni.Background = Brushes.White;
                    ((TextBlock)pocetni.Children[0]).Text = string.Empty;
                    pocetni.Resources.Remove("taken");
                }

                draggedItem = null; 
                dragging = false;
             }
        }

       
        private void Oslobodi_Dugme(Canvas roditelj)
        {
            if (roditelj.Resources["taken"] != null)
            {
                VratiElement(roditelj);

                roditelj.Background = Brushes.White;
                ((TextBlock)roditelj.Children[0]).Text = string.Empty;
                roditelj.Resources.Remove("taken");


            } 
        }

        private void VratiElement(Canvas roditelj)
        {
            string naziv_entiteta = ((TextBlock)roditelj.Children[0]).Text;

            Server item = null;
            KlasifikovaniEntiteti klasa_u_kojoj_se_nalaze = null;

            int brojac_klase = 0;

            foreach(KlasifikovaniEntiteti klasifikovani in EntitetiCanvas)
            {
                foreach(Server s in klasifikovani.ListaEntiteta)
                {
                    if (s.Naziv.Equals(naziv_entiteta))
                    {
                        //pronasli smo entitet
                        klasa_u_kojoj_se_nalaze = klasifikovani;
                        item = s;
                        EntitetiTreeView[brojac_klase].ListaEntiteta.Add(item);
                        break;
                    }
                }

                brojac_klase++; //prelazimo na sledecu adresnu klasu
            }

                if(item == null || klasa_u_kojoj_se_nalaze == null || brojac_klase > 4)
                {
                     return;
                }

            //u pronadjenoj klasi za pronadjeni entiet - ukloniti referencu
            item.Canvas_pozicija = -1;

            klasa_u_kojoj_se_nalaze.ListaEntiteta.Remove(item);
                
        }

        private void Promena_SelectedItemChanged(TreeView tv)
        {
           var prozor = RasporedMrezeView.UserControl;

           if (!dragging && tv != null && tv.SelectedItem != null && tv.SelectedItem.GetType() == typeof(Server))
            {
              dragging = true;
               draggedItem = (Server)tv.SelectedItem;
               selected = PronadjiElement(draggedItem);
             DragDrop.DoDragDrop(prozor, draggedItem, DragDropEffects.Move | DragDropEffects.Copy);
           }
        }

        private int PronadjiElement(Server draggedItem)
        {
            int index = 0;
            if (draggedItem.Klasa.Equals("Server_1"))
            {
                index = EntitetiTreeView[0].ListaEntiteta.IndexOf(draggedItem);
            }
            else if (draggedItem.Klasa.Equals("Server_2"))
            {
                index = EntitetiTreeView[1].ListaEntiteta.IndexOf(draggedItem);
            }
            else if (draggedItem.Klasa.Equals("Server_3"))
            {
                index = EntitetiTreeView[2].ListaEntiteta.IndexOf(draggedItem);
            }
            else if (draggedItem.Klasa.Equals("Server_4"))
            {
                index = EntitetiTreeView[3].ListaEntiteta.IndexOf(draggedItem);
            }
            else if (draggedItem.Klasa.Equals("Server_5"))
            {
                index = EntitetiTreeView[4].ListaEntiteta.IndexOf(draggedItem);
            }

            return index;
        }

        private void TreeView_MouseLeftButtonUp()
        {
           dragging = false;
            draggedItem = null;
        }

        private void DropMetoda(Canvas kanvas)
        {
            TextBlock ispis = ((TextBlock)(kanvas).Children[0]);

            if(draggedItem != null)
            {
                if (kanvas.Resources["taken"] == null)
                {
                    BitmapImage img = new BitmapImage();
                    img.BeginInit();
                    string putanja = "pack://application:,,,/Assets/device.png";
                    img.UriSource = new Uri(putanja, UriKind.Absolute);
                    img.EndInit();
                    kanvas.Background = new ImageBrush(img);
                    ispis.Text = draggedItem.Naziv;
                    ispis.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#000000"));
                    draggedItem.Canvas_pozicija = GetCanvasId(kanvas.Name);
                    kanvas.Resources.Add("taken", true);
                    UkloniElement(draggedItem);
                }

                draggedItem = null;
                dragging = false;
            }
        }

        private void UkloniElement(Server draggedItem)
        {
            if(draggedItem.Klasa.Equals("Server_1")) 
            {
                EntitetiTreeView[0].ListaEntiteta.RemoveAt(selected);
                EntitetiCanvas[0].ListaEntiteta.Add(draggedItem);
            }else if(draggedItem.Klasa.Equals("Server_2"))
            {
                EntitetiTreeView[1].ListaEntiteta.RemoveAt(selected);
                EntitetiCanvas[1].ListaEntiteta.Add(draggedItem);

            }
            else if (draggedItem.Klasa.Equals("Server_3"))
            {
                EntitetiTreeView[2].ListaEntiteta.RemoveAt(selected);
                EntitetiCanvas[2].ListaEntiteta.Add(draggedItem);
            }
            else if (draggedItem.Klasa.Equals("Server_4"))
            {
                EntitetiTreeView[3].ListaEntiteta.RemoveAt(selected);
                EntitetiCanvas[3].ListaEntiteta.Add(draggedItem);
            }
            else if (draggedItem.Klasa.Equals("Server_5"))
            {
                EntitetiTreeView[4].ListaEntiteta.RemoveAt(selected);
                EntitetiCanvas[4].ListaEntiteta.Add(draggedItem);
            }
        }

        private void Rasporedi(Grid desni_grid_canvas)
        {


            // Rasporedi na preostala slobodna mesta
            for (int i = 1; i <= 12; i++)
            {
                // uzmemo canvas
                Canvas kanvas = ((Canvas)((DockPanel)(desni_grid_canvas.Children[i])).Children[1]);
                TextBlock trenutni = (TextBlock)((kanvas).Children[0]);
                string naziv_entiteta = trenutni.Text.Trim();

                if (naziv_entiteta.Equals(""))
                {
                    // prazan je canvas
                    if (EntitetiTreeView[0].ListaEntiteta.Count > 0)
                    {
                        draggedItem = EntitetiTreeView[0].ListaEntiteta[0];
                        EntitetiTreeView[0].ListaEntiteta.RemoveAt(0);
                        EntitetiCanvas[0].ListaEntiteta.Add(draggedItem);
                    }
                    else if (EntitetiTreeView[1].ListaEntiteta.Count > 0)
                    {
                        draggedItem = EntitetiTreeView[1].ListaEntiteta[0];
                        EntitetiTreeView[1].ListaEntiteta.RemoveAt(0);
                        EntitetiCanvas[1].ListaEntiteta.Add(draggedItem);
                    }
                    else if (EntitetiTreeView[2].ListaEntiteta.Count > 0)
                    {
                        draggedItem = EntitetiTreeView[2].ListaEntiteta[0];
                        EntitetiTreeView[2].ListaEntiteta.RemoveAt(0);
                        EntitetiCanvas[2].ListaEntiteta.Add(draggedItem);
                    }
                    else if (EntitetiTreeView[3].ListaEntiteta.Count > 0)
                    {
                        draggedItem = EntitetiTreeView[3].ListaEntiteta[0];
                        EntitetiTreeView[3].ListaEntiteta.RemoveAt(0);
                        EntitetiCanvas[3].ListaEntiteta.Add(draggedItem);
                    }
                    else if (EntitetiTreeView[4].ListaEntiteta.Count > 0)
                    {
                        draggedItem = EntitetiTreeView[4].ListaEntiteta[0];
                        EntitetiTreeView[4].ListaEntiteta.RemoveAt(0);
                        EntitetiCanvas[4].ListaEntiteta.Add(draggedItem);
                    }

                    if (draggedItem != null)
                    {
                        draggedItem.Canvas_pozicija = i; // pozicija na canvasu

                        if (kanvas.Resources["taken"] == null)
                        {
                            BitmapImage img = new BitmapImage();
                            img.BeginInit();
                            // string putanja = Directory.GetCurrentDirectory() + "/Assets/uredjaj.png";
                            string putanja = "pack://application:,,,/Assets/device.png";
                            img.UriSource = new Uri(putanja, UriKind.Absolute);
                            img.EndInit();
                            kanvas.Background = new ImageBrush(img);
                            trenutni.Text = draggedItem.Naziv;
                            trenutni.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#000000"));
                            draggedItem.Canvas_pozicija = GetCanvasId(kanvas.Name);
                            kanvas.Resources.Add("taken", true);
                            //UkloniElement(draggedItem);
                        }
                        draggedItem = null;
                        dragging = false;
                    }
                }
            }
        
        }

        private void DragOverMetoda(Canvas obj)
        {
           //to do
        }

        private int GetCanvasId(string name)
        {
            int id = 1;

            if (name.Equals("c1")) id = 1;
            if (name.Equals("c2")) id = 2;
            if (name.Equals("c3")) id = 3;
            if (name.Equals("c4")) id = 4;
            if (name.Equals("c5")) id = 5;
            if (name.Equals("c6")) id = 6;
            if (name.Equals("c7")) id = 7;
            if (name.Equals("c8")) id = 8;
            if (name.Equals("c9")) id = 9;
            if (name.Equals("c10")) id = 10;
            if (name.Equals("c11")) id = 11;
            if (name.Equals("c12")) id = 12;

            return id;
        }
        
       
        public void InicijalizacijaListi()
        {
            EntitetiCanvas = new BindingList<KlasifikovaniEntiteti>()
            {
                new KlasifikovaniEntiteti() { KlasaServera = "Klasa Servera 1" },
                new KlasifikovaniEntiteti() { KlasaServera = "Klasa Servera 2" },
                new KlasifikovaniEntiteti() { KlasaServera = "Klasa Servera 3" },
                new KlasifikovaniEntiteti() { KlasaServera = "Klasa Servera 4" },
                new KlasifikovaniEntiteti() { KlasaServera = "Klasa Servera 5" }
            };

            EntitetiTreeView = new BindingList<KlasifikovaniEntiteti>()
            {
                new KlasifikovaniEntiteti() { KlasaServera = "Klasa Servera 1" },
                new KlasifikovaniEntiteti() { KlasaServera = "Klasa Servera 2" },
                new KlasifikovaniEntiteti() { KlasaServera = "Klasa Servera 3" },
                new KlasifikovaniEntiteti() { KlasaServera = "Klasa Servera 4" },
                new KlasifikovaniEntiteti() { KlasaServera = "Klasa Servera 5" }
            };
        }
    }
}
