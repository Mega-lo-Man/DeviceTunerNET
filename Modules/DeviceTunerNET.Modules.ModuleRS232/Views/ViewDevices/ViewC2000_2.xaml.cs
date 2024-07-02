using DeviceTunerNET.Modules.ModulePnr.ViewModels.DeviceViewModels;
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

namespace DeviceTunerNET.Modules.ModulePnr.Views.ViewDevices
{
    /// <summary>
    /// Interaction logic for ViewC2000_2.xaml
    /// </summary>
    public partial class ViewC2000_2 : UserControl
    {
        public ViewC2000_2()
        {
            InitializeComponent();
            DataContext = new C2000_2ViewModel();
        }
    }
}
