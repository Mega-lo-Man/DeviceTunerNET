using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DeviceTunerNET.Modules.ModuleSwitch.ViewModels
{
    [ValueConversion(typeof(string), typeof(BitmapImage))]
    public class DesignSwitchConverter : IValueConverter
    {
        public BitmapImage ImagePathOK { get; set; }
        public BitmapImage ImagePathCancel { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is string)
            {
                bool tmp = false;
                string str = (string)value;

                if (str.Length > 0) tmp = true;
                return tmp || DesignerProperties.GetIsInDesignMode(new DependencyObject())
                    ? ImagePathOK
                    : ImagePathCancel;
                /*return (tmp) || DesignerProperties.GetIsInDesignMode(new DependencyObject())
                    ? new SolidColorBrush(Colors.Green)
                    : new SolidColorBrush(Colors.Red);
                */
            }
            return ImagePathCancel;//new SolidColorBrush(Colors.Aqua);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
