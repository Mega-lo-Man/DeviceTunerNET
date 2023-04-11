using DeviceTunerNET.SharedDataModel.Devices;

namespace DeviceTunerNET.SharedDataModel.ElectricModules
{
    public class MechanicalRelay : OptoRelay
    {
        public MechanicalRelay(IOrionDevice orionDevice, byte relayIndex) : base(orionDevice, relayIndex)
        {
        }

        public enum OutputTypes : byte
        {
            Standard = 1,
            Auxilary = 2,
            FireEquiped = 3
        }

        public OutputTypes OutputType { get; set; } = OutputTypes.Standard;
    }
}
