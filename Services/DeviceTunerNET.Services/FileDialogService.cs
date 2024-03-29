﻿using DeviceTunerNET.Services.Interfaces;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Windows;

namespace DeviceTunerNET.Services
{
    public class FileDialogService : IFileDialogService, IMessageService
    {

        private string _fullFileNames;
        public string FullFileNames => _fullFileNames;

        public event Action<string> DataArrived;

        public void AddData(string newData)
        {

        }

        public string GetMessage()
        {
            throw new NotImplementedException();
        }

        public bool OpenFileDialog()
        {
            var openfileDlg = new CommonOpenFileDialog();
            openfileDlg.Title = "MyTitle";
            if (openfileDlg.ShowDialog() != CommonFileDialogResult.Ok) 
                return false;

            _fullFileNames = openfileDlg.FileName;

            return true;
        }

        public bool SaveFileDialog()
        {
            throw new System.NotImplementedException();
        }

        public void SendMessage(string sms)
        {
            throw new NotImplementedException();
        }

        public void ShowMessage(string message)
        {
            MessageBox.Show("You selected: " + message);
        }

    }
}
