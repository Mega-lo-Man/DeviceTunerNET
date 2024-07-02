using DeviceTunerNET.Services.Interfaces;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DeviceTunerNET.Services
{
    public class DialogCaller(IDialogService dialogService) : IDialogCaller
    {
        private readonly IDialogService _dialogService = dialogService;

#pragma warning disable CA1416 // Validate platform compatibility
        private static DialogParameters GetSerialDialogParams(string model, string designation)
        {
            return new DialogParameters
                {
                    {"title", "Ввод серийного номера."},
                    {"message", "Серийник: "},
                    {"model", model},
                    {"designation", designation}
                };
        }

        public string GetSerialNumber(string model, string designation)
        {
            var manualReset = new ManualResetEvent(false);
            var serialNumber = string.Empty;
            var result = System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var parameters = GetSerialDialogParams(model, designation);

                _dialogService.ShowDialog("SerialDialog", parameters, dialogResult =>
                {
                    if (dialogResult.Result == ButtonResult.OK
                        && dialogResult.Parameters.ContainsKey("Serial"))
                    {
                        serialNumber = dialogResult.Parameters.GetValue<string>("Serial");
                    }
                    manualReset.Set();
                });
            }));
            manualReset.WaitOne();
            return serialNumber;
        }

        public void ShowMessage(string message)
        {
            MessageBox.Show(message);
        }
#pragma warning restore CA1416 // Validate platform compatibility
    }
}
