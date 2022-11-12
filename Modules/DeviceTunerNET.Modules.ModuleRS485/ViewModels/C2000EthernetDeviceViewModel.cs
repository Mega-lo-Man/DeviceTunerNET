using DeviceTunerNET.Core;
using DeviceTunerNET.Core.Mvvm;
using DeviceTunerNET.SharedDataModel;
using DeviceTunerNET.SharedDataModel.Devices;
using Prism.Events;

namespace DeviceTunerNET.Modules.ModuleRS485.ViewModels
{
    public class C2000EthernetDeviceViewModel : TreeViewItemViewModel
    {
        private readonly C2000Ethernet _device;
        private readonly IEventAggregator _ea;

        public C2000EthernetDeviceViewModel(C2000Ethernet device, CabinetViewModel cabinetParent, IEventAggregator ea)
            : base(cabinetParent, false)
        {
            _device = device;
            _ea = ea;
        }

        public string GetDeviceDesignation => _device.Designation;

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
