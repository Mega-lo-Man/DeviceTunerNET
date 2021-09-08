namespace DeviceTunerNET.SharedDataModel
{
    public class EthernetSwitch : RS232device
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
