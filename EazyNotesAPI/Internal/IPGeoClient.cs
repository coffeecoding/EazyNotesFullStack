using System;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace EazyNotesAPI.Internal
{
    public class IPGeoData
    {
        public string Ip { get; set; }
        public string Hostname { get; set; }
        public string Country_code { get; set; }
        public string Country_name { get; set; }
        public string Region_code { get; set; }
        public string Region_name { get; set; }
        public string City { get; set; }
        // NOTE: SECURITY IS NOT SUPPORTED WITH THE FREE IpStack API KEY !!!
        public IPSecurityInfo Security { get; set; }
    }

    public class IPSecurityInfo
    {
        public bool Is_proxy { get; set; }
        public string Proxy_type { get; set; }
        public bool Is_crawler { get; set; }
        public bool Is_tor { get; set; }
        public string Threat_level { get; set; }
    }

    public class IPGeoClient
    {
        static string APIBaseUrl = @"http://api.ipstack.com/";
        static string APIKey;

        static HttpClient _client;

        public static void InitClient(string apikey)
        {
            APIKey = apikey;
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(APIBaseUrl)
            };
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client = client;
        }

        public static async Task<IPGeoData> GetGeolocationByIP(string ip)
        {
            try
            {
                var response = await _client.GetAsync($"{APIBaseUrl}/{ip}?access_key={APIKey}");
                var json = await response.Content.ReadAsStringAsync();
                IPGeoData result = JsonConvert.DeserializeObject<IPGeoData>(json);
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to grab IP reverse geo data for IP {ip}. Reason:");
                Console.WriteLine($"{e.GetType()}: {e.Message}");
                return new IPGeoData();
            }
        }
    }
}
