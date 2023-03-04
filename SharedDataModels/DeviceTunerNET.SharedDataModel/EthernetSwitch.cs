namespace DeviceTunerNET.SharedDataModel
{
    public class EthernetSwitch : EthernetOrionDevice
    {
        public EthernetSwitch(IPort port) : base(port)
        {
        }

        /// <summary>
        /// LogIn user name
        /// </summary>
        public string Username { get; set; } = "";

        /// <summary>
        /// LogIn user password
        /// </summary>
        public string Password { get; set; } = "";
    }
}
