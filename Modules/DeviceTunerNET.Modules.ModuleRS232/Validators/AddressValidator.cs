using System;
using System.Globalization;
using System.Windows.Controls;

namespace DeviceTunerNET.Modules.ModuleRS232.Validators
{
    public class AddressValidator : ValidationRule
    {
        private const int MAX_ADDRESS = 127;
        private const int MIN_ADDRESS = 0;

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            ValidationResult result = new ValidationResult(true, null);
            string inputString = (value ?? string.Empty).ToString();

            if (Int32.TryParse(inputString, out int int32str))
            {
                if (int32str <= MIN_ADDRESS || int32str >= MAX_ADDRESS)
                {
                    result = new ValidationResult(false, this.ErrorMessage);
                }
            }

            return result;
        }
    }
}
