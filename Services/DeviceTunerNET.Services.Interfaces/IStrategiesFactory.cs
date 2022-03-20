using DeviceTunerNET.Services.Interfaces;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface IStrategiesFactory
    {
        ISwitchConfigUploader GetInstance<T>() where T : ISwitchConfigUploader;
    }
}