namespace DeviceTunerNET.Core
{
    public class Message
    {
        public int ActionCode { get; set; }
        public string MessageString { get; set; }
        public object AttachedObject { get; set; }
    }
}
