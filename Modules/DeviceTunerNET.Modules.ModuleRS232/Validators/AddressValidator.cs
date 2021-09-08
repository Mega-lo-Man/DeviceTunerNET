using System.Globalization;
using System.Windows.Controls;
using static System.Int32;

namespace DeviceTunerNET.Modules.ModuleRS232.Validators
{
    public class AddressValidator : ValidationRule
    {
        private const int MAX_ADDRESS = 127;
        private const int MIN_ADDRESS = 0;

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => _errorMessage = value;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var result = new ValidationResult(true, null);
            var inputString = (value ?? string.Empty).ToString();

            if (!TryParse(inputString, out var int32str))
                return result;

            if (int32str <= MIN_ADDRESS || int32str >= MAX_ADDRESS)
            {
                result = new ValidationResult(false, ErrorMessage);
            }

            return result;
        }
    }
}
