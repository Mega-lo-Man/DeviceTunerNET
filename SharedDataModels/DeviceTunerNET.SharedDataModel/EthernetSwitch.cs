namespace DeviceTunerNET.SharedDataModel
{
    public class EthernetSwitch : EthernetOrionDevice
    {
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
