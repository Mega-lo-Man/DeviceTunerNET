using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel
{
    public class EthernetSwitch : RS232device
    {
        /// <summary>
        /// LogIn user name
        /// </summary>
        private string _username = "";
        public string Username 
        {
            get { return _username; }
            set { _username = value; }
        }

        /// <summary>
        /// LogIn user password
        /// </summary>
        private string _password = "";
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }
    }
}
