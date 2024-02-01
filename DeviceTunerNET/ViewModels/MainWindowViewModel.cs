using DeviceTunerNET.Services.Interfaces;
using Prism.Commands;
using Prism.Mvvm;
using System;

namespace DeviceTunerNET.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Шей да пори!";
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private readonly IFileDialogService _dialogService;
        private readonly IDataRepositoryService _dataRepositoryService;

        public DelegateCommand OpenFileCommand { get; }
        public DelegateCommand SaveFileCommand { get; }
        public DelegateCommand CloseAppCommand { get; }

        public MainWindowViewModel(IFileDialogService dialogService, IDataRepositoryService dataRepositoryService)
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
            if (!_dialogService.OpenFileDialog()) 
                return;

            var selectedFile = _dialogService.FullFileNames; // Путь к Excel-файлу
            // 1 - Поставщик данных - Excel
            _dataRepositoryService.SetDevices(1, selectedFile); //Устанавливаем список всех устройств в репозитории
        }
    }
}
