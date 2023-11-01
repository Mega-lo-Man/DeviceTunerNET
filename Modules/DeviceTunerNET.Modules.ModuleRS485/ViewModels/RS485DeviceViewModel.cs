using DeviceTunerNET.Core;
using DeviceTunerNET.Core.Mvvm;
using DeviceTunerNET.SharedDataModel;
using Prism.Events;

namespace DeviceTunerNET.Modules.ModuleRS485.ViewModels
{
    class RS485DeviceViewModel : TreeViewItemViewModel
    {
        private readonly IOrionDevice _device;
        private readonly IEventAggregator _ea;

        public RS485DeviceViewModel(IOrionDevice device, CabinetViewModel cabinetParent, IEventAggregator ea)
            : base(cabinetParent, false)
        {
            _ea = ea;
            _device = device;
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
