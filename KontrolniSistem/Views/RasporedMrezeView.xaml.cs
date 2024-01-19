using KontrolniSistem.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KontrolniSistem.Views
{
    /// <summary>
    /// Interaction logic for RasporedMrezeView.xaml
    /// </summary>
    public partial class RasporedMrezeView : UserControl
    {

        public static UserControl UserControl {  get; set; }
        public RasporedMrezeView()
        {
            InitializeComponent();

            UserControl = this;

            DataContext = new RasporedMrezeViewModel(leviGridSaCanvas);
        }
    }
}
