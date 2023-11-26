using DeviceTunerNET.Modules.ModulePnr.Interfaces;
using DeviceTunerNET.SharedDataModel.ElectricModules;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace DeviceTunerNET.Modules.ModulePnr.ViewModels
{
    public class ViewSingleShleifViewModel : BindableBase, IControlViewModel
    {
        private string _labelText = string.Empty;
        private int _xCounter = 0;
        public string LabelText
        {
            get => _labelText;
            set
            {
                _labelText = value;
                SetProperty(ref _labelText, value);
            }
        }

        private bool _isControlEnabled = false;
        public bool IsControlEnabled
        {
            get => _isControlEnabled;
            set
            {
                _isControlEnabled = value;
                SetProperty(ref _isControlEnabled, value);
            }
        }

        private bool _isActive = false;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                SetProperty(ref _isActive, value);
            }
        }

        private int _minValue = 0;
        public int MinValue
        {
            get => _minValue;
            set
            {
                _minValue = value;
                SetProperty(ref _minValue, value);
            }
        }

        private int _maxValue = 255;
        public int MaxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = value;
                SetProperty(ref _maxValue, value);
            }
        }

        public ObservableCollection<KeyValuePair<string, int>> DataPoints { get; set; } = new();

        public Shleif ShleifInstance { get; set; }

        public ViewSingleShleifViewModel(Shleif shleif)
        {
            ShleifInstance = shleif;
            LabelText = ShleifInstance.ShleifIndex.ToString();
            var random = new Random();
            DataPoints.Clear();
            for (int i = 0; i < 10;  i++)
            {
                DataPoints.Add(new KeyValuePair<string, int>(_xCounter.ToString(), random.Next(10, 100)));
                _xCounter++;
            }
            
            
        }

    }
}
