using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Modules.ModuleRS232.ViewModels
{
    public class ViewRS232ViewModel : BindableBase
    {
        private string _message;

        public string Title { get; private set; }

        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        public ViewRS232ViewModel()
        {
            Title = "RS232";
            Message = "View A from your Prism Module";
        }
    }
}
