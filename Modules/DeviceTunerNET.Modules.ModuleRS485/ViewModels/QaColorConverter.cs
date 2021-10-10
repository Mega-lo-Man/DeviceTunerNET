using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DeviceTunerNET.Modules.ModuleRS485.ViewModels
{
    public class QaColorConverter : IValueConverter
    {
        public BitmapImage ImagePathOk { get; set; }
        public BitmapImage ImagePathCancel { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool qaStatus))
                return ImagePathCancel; //new SolidColorBrush(Colors.Aqua);
            

            return qaStatus || DesignerProperties.GetIsInDesignMode(new DependencyObject())
                ? ImagePathOk
                : ImagePathCancel;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}