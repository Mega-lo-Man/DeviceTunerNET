using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface IDialogService
    {

        /// <summary>
        /// Full names of the selected files
        /// </summary>
        string FullFileNames { get; }


        bool OpenFileDialog();  // открытие файла
        bool SaveFileDialog();  // сохранение файла
    }
}
