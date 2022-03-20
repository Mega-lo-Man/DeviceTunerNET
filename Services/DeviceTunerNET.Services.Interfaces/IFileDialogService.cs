namespace DeviceTunerNET.Services.Interfaces
{
    public interface IFileDialogService
    {

        /// <summary>
        /// Full names of the selected files
        /// </summary>
        string FullFileNames { get; }


        bool OpenFileDialog();  // открытие файла
        bool SaveFileDialog();  // сохранение файла
    }
}
