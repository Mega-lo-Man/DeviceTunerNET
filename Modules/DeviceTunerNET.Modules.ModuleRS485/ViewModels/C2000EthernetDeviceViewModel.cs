using DeviceTunerNET.Core;
using DeviceTunerNET.Core.Mvvm;
using DeviceTunerNET.SharedDataModel;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Modules.ModuleRS485.ViewModels
{
    public class C2000EthernetDeviceViewModel : TreeViewItemViewModel
    {
        private C2000Ethernet _device;
        private IEventAggregator _ea;

        public C2000EthernetDeviceViewModel(C2000Ethernet device, CabinetViewModel cabinetParent, IEventAggregator ea)
            : base(cabinetParent, false)
        {
            _device = device;
            _ea = ea;
        }

        public string GetDeviceDesignation
        {
            get { return _device.Designation; }
        }

        protected override void OnSelectedItemChanged()
        {
            //base.OnSelectedItemChanged();
            //Сообщаем всем об том, что юзер кликнул на объекте в дереве.
            //Объект на который юзер кликнул передаётся через свойство AttachedObject
            _ea.GetEvent<MessageSentEvent>().Publish(new Message
            {
                ActionCode = MessageSentEvent.UserSelectedItemInTreeView,
                AttachedObject = _device
            });
        }
    }
}
