namespace DeviceTunerNET.Services.Interfaces
{
    public interface IMessageService
    {
        void SendMessage(string sms);
        string GetMessage();
        event System.Action<string> DataArrived;
    }
}
