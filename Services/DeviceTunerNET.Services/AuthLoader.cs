using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Management;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Diagnostics;
using DeviceTunerNET.Services.Interfaces;

namespace DeviceTunerNET.Services
{
    public class AuthLoader() : IAuthLoader
    {
        private string _serial = string.Empty;

        public IEnumerable<string> AvailableServicesNames { get; set; } = Enumerable.Empty<string>();

        public async Task<IEnumerable<string>> GetAvailableServices()
        {
            _serial = GetCurrentMotherboardSerial();

            return await FetchDataAsync();
        }

        private async Task<IEnumerable<string>> FetchDataAsync()
        {
            try
            {
                var email = "texview@yandex.ru";
                var password = "shap@8525";

                var loginData = new
                {
                    email,
                    password,
                };

                var loginUrl = "http://www.elatale.site:3001/api/auth/login";
                using var httpClient = new HttpClient();
                var loginResponse = await httpClient.PostAsJsonAsync(loginUrl, loginData);
                loginResponse.EnsureSuccessStatusCode();

                var token = (await loginResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString();

                var headers = new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {token}" },
                };

                var hexSerial = Uri.EscapeDataString(_serial);
                var deviceUrl = $"http://www.elatale.site:3001/api/hardware/?serial={_serial}";
                var request = new HttpRequestMessage(HttpMethod.Get, deviceUrl);

                foreach(var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                var deviceResponse = await httpClient.SendAsync(request);
                deviceResponse.EnsureSuccessStatusCode();

                var deviceData = await deviceResponse.Content.ReadFromJsonAsync<AvailableServicesDto>();
                AvailableServicesNames = GetListOfAvailableServices(deviceData.Functionalities);

                return AvailableServicesNames;
            }
            catch (HttpRequestException e)
            {
                Debug.WriteLine(e);
                return Enumerable.Empty<string>();
            }
        }

        private static IEnumerable<string> GetListOfAvailableServices(IEnumerable<Functionality> functionalities)
        {
            foreach (var functionality in functionalities)
            {
                yield return functionality.Name;
            }
        }

        private static string GetCurrentMotherboardSerial()
        {
            var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
            var information = searcher.Get();

            var motherBoardSerial = "";
            foreach(var m in information)
            {
                motherBoardSerial = m["SerialNumber"].ToString();
            }
            return motherBoardSerial;
        }

        private record AvailableServicesDto()
        {
            public int Id { get; set; }
            public string Serial { get; set; }
            public string Description { get; set; }
            public Functionality[] Functionalities { get; set; }
        }

        private record Functionality
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }
    }
}
