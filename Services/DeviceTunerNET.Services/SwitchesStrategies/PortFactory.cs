using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services.SwitchesStrategies
{
    public class PortFactory
    {
        public PortFactory()
        {
        }

        public IEnumerable<SwitchPort> GetSwitchPorts(string switchResponse)
        {
            var portLines = SplitResponseByLine(switchResponse);
            var columnsInfo = GetColumnsShifting(portLines.First());
            for(int i = 1; i < portLines.Count(); i++)
            {
                var switchPort = GetSwitchPort(GetColumnsOfSingleLine(portLines.ElementAt(i), columnsInfo));
                yield return switchPort;
            }           
        }

        private SwitchPort GetSwitchPort(IEnumerable<string> portLine)
        {
            var switchPort = new SwitchPort();

            switchPort.Port = portLine.ElementAt(0);
            switchPort.Type = portLine.ElementAt(1);
            switchPort.Duplex = portLine.ElementAt(2);

            int.TryParse(portLine.ElementAt(3), out var portSpeed);
            switchPort.Speed = portSpeed;

            switchPort.Neg = portLine.ElementAt(4);
            switchPort.FlowControl = portLine.ElementAt(5);
            switchPort.LinkState = portLine.ElementAt(6);
            switchPort.Uptime = portLine.ElementAt(7);
            switchPort.BackPressure = portLine.ElementAt(8);
            switchPort.MdixMode = portLine.ElementAt(9);
            switchPort.PortMode = portLine.ElementAt(10);

            int.TryParse(portLine.ElementAt(11), out var vlan);
            switchPort.VLAN = vlan;

            return switchPort;
        }


        // Весь ответ надо разбить на строки.
        // Первая строка - сама команда.
        // Вторая строка - блоки тире с пробелами.
        // Каждая последующая строка - хар-ки отдельного порта.
        // Последняя строка - мусор из тире и пробелов (полностью удалить)
        public IEnumerable<string> SplitResponseByLine(string switchResponse)
        {
            var portLines = switchResponse.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            portLines.RemoveAt(0); // удаляем саму команду
            portLines.RemoveAt(portLines.Count - 1); // удаляем последнюю строку с мусором

            return portLines;
        }


        public IEnumerable<int> GetColumnsShifting(string str)
        {
            var columns = new List<int>();
            var lastSymbol = ' ';
            for (var i = 0; i < str.Count(); i++)
            {
                var currentSymbol = str.ElementAt(i);
                if (lastSymbol == ' ' && currentSymbol == '-')
                {
                    yield return i;
                }
                lastSymbol = currentSymbol;
            }
            yield return str.Count();
        }

        public IEnumerable<string> GetColumnsOfSingleLine(string str, IEnumerable<int> colShifting)
        {
            var colCount = colShifting.Count() - 1;

            for (var i = 0; i < colCount; i++)
            {
                // Длина str не совпадает с длиной colShifting,
                // в последней итеррации надо ориентироваться на последний символ в строке str
                if (i < colCount - 1)
                {
                    var length = colShifting.ElementAt(i + 1) - colShifting.ElementAt(i);
                    yield return str.Substring(colShifting.ElementAt(i), length).Trim();
                    continue;
                }
                yield return str.Substring(colShifting.ElementAt(i), str.Length - colShifting.ElementAt(i)).Trim();
            }
        }

        public IEnumerable<string> SeparatePortMode(IEnumerable<string> portSettings)
        {
            var settings = portSettings.ToList();
            var str = settings.Last();
            settings.RemoveAt(settings.Count() - 1);

            var portMode = str.Split(' ');
            // Извлечение номера VLAN из строки со скобками
            var vlanStr = portMode.Last().Where(Char.IsDigit).ToArray();

            settings.Add(portMode.First());

            if (vlanStr == null)
                settings.Add("");

            settings.Add(new string(vlanStr));

            return settings;
        }
    }
}
