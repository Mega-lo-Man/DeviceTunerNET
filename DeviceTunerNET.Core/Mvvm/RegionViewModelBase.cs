using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Core.Mvvm
{
    public class RegionViewModelBase : ViewModelBase, INavigationAware, IConfirmNavigationRequest
    {
        #region Properties
        string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
        #endregion

        protected IRegionManager RegionManager { get; private set; }

        public RegionViewModelBase(IRegionManager regionManager)
        {
            RegionManager = regionManager;
        }

        public virtual void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
        {
            continuationCallback(true);
        }

        public virtual bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {

        }
    }
}
