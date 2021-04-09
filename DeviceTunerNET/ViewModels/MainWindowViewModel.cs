using DeviceTunerNET.Services.Interfaces;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;

namespace DeviceTunerNET.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Prism Application";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private Services.Interfaces.IDialogService _dialogService;
        private IDataRepositoryService _dataRepositoryService;

        public DelegateCommand OpenFileCommand { get; private set; }
        public DelegateCommand SaveFileCommand { get; private set; }
        public DelegateCommand CloseAppCommand { get; private set; }

        public MainWindowViewModel(Services.Interfaces.IDialogService dialogService, IDataRepositoryService dataRepositoryService)
        {
            _dialogService = dialogService;
            _dataRepositoryService = dataRepositoryService;

            OpenFileCommand = new DelegateCommand(OpenFileExecute, OpenFileCanExecute);
            SaveFileCommand = new DelegateCommand(SaveFileExecute, SaveFileCanExecute);
            CloseAppCommand = new DelegateCommand(CloseAppExecute, CloseAppCanExecute);
        }

        private bool CloseAppCanExecute()
        {
            return true;
        }

        private void CloseAppExecute()
        {
            throw new NotImplementedException();
        }

        private bool SaveFileCanExecute()
        {
            return true;
        }

        private void SaveFileExecute()
        {
            _dialogService.SaveFileDialog();
        }

        private bool OpenFileCanExecute()
        {
            return true;
        }

        private void OpenFileExecute()
        {
            if (_dialogService.OpenFileDialog())
            {
                string selectedFile = _dialogService.FullFileNames; // Путь к Excel-файлу
                // 1 - Поставщик данных - Excel
                _dataRepositoryService.SetDevices(1, selectedFile); //Устанавливаем список всех устройств в репозитории
            }
        }
    }
}
