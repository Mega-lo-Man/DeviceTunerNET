namespace DeviceTunerNET.SharedDataModel
{
    public class EthernetSwitch : CommunicationDevice, IEthernetDevice
    {
        public EthernetSwitch(IPort port)
        {
            Port = port;
        }

        /// <summary>
        /// LogIn user name
        /// </summary>
        public string Username { get; set; } = "";

        /// <summary>
        /// LogIn user password
        /// </summary>
        public string Password { get; set; } = "";
        public string AddressIP { get; set; }
        public string Netmask { get; set; }
        public string MACaddress { get; set; }
        public string DefaultGateway { get; set; }
        public int CIDR
        {
            get => EthernetUtils.ConvertToCidr(Netmask);
            set => Netmask = EthernetUtils.CidrToString(value);
        }

    }
}
