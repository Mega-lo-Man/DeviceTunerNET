using DeviceTunerNET.Core;
using DeviceTunerNET.Core.Mvvm;
using DeviceTunerNET.SharedDataModel;
using Prism.Events;
using System;

namespace DeviceTunerNET.Modules.ModuleRS485.ViewModels
{
    public class CabinetViewModel : TreeViewItemViewModel
    {
        private readonly Cabinet _cabinet;
        private readonly IEventAggregator _ea;

        public CabinetViewModel(Cabinet cabinet, IEventAggregator ea) : base(null, true)
        {
            _ea = ea;
            _cabinet = cabinet;
        }

        public string GetCabinetDesignation => _cabinet.Designation;

        protected override void LoadChildren()
        {
            foreach (var device in _cabinet.GetDevicesList<RS485device>())
            {
                Children.Add(new RS485DeviceViewModel(device, this, _ea));
            }
            foreach (var item in _cabinet.GetDevicesList<C2000Ethernet>())
            {
                Children.Add(new C2000EthernetDeviceViewModel(item, this, _ea));
            }
        }

        protected override void OnSelectedItemChanged()
        {
            //base.OnSelectedItemChanged();
            //Сообщаем всем об том, что юзер кликнул на объекте в дереве.
            //Объект на который юзер кликнул передаётся через свойство AttachedObject
            _ea.GetEvent<MessageSentEvent>().Publish(new Message
            {
                ActionCode = MessageSentEvent.UserSelectedItemInTreeView,
                AttachedObject = _cabinet
            });
        }
    }
}
