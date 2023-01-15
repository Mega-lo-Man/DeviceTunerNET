using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface IConfigParser
    {
        public enum Errors
        {
            UnknownError = 0,
            Ok = 1,
            IoException = 2,
            UnknownVariable = 3,
            FileNotFound = 4,
            VariablesNotFound = 5,
            SecurityException = 6
            
        }

        /// <summary>
        /// Get all unrecognized parsing variables.
        /// </summary>
        public IEnumerable<string> GetErrorsVariables { get; set; }

        /// <summary>
        /// Parse template config file.
        /// </summary>
        /// <param name="variables">Dictionary with variables for replacement in template</param>
        /// <param name="templateConfigPath">Template config path (template for parsing)</param>
        /// <param name="outputConfigPath">Output config file to download to the switch</param>
        /// <returns></returns>
        public Errors Parse(Dictionary<string, string> variables, string templateConfigPath, string outputConfigPath);
        
        
    }
}
