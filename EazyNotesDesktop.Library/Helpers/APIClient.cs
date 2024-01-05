using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using EazyNotes.Models.DTO;
using EazyNotes.Models.POCO;
using EazyNotes.CryptoServices;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Http.Connections.Client;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace EazyNotesDesktop.Library.Helpers
{
    public class APIClient : DataClient
    {
        //const string ApiAddress = "http://localhost:5000";
        //const string APIBaseUrl = "http://enbeta.aoe2bc.com";
        static string APIBaseUrl() => AppSettings.GetSection("APIBaseUrl").Value;

        private HttpClient httpClient;
        public HubConnection DataHubConnection { get; private set; }
        private static IConfiguration AppSettings;

        public APIClient(IUserData userData) : base(userData)
        {
            if (AppSettings == null)
            {
                AppSettings = new ConfigurationBuilder()
                    .SetBasePath(Environment.CurrentDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();
            }
            InitializeClient();
        }

        public override void Logout()
        {
            httpClient?.DefaultRequestHeaders.Clear();
            if (DataHubConnection != null)
                DataHubConnection.StopAsync();
            base.Logout();
        }

        private void InitializeClient()
        {
            // TODO: Refactor into configuration file (json)
            httpClient = new HttpClient
            {
                BaseAddress = new Uri(APIBaseUrl())
            };
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Assumes that a valid JWT token was already added to the httpClientHeader and uses that one for SignalR.
        /// </summary>
        private void InitializeSignalRClient()
        {
            string tokenValue = httpClient.DefaultRequestHeaders.GetValues("Authorization").Single();
            // Initialize websocket(s)
            void websocketOptions(HttpConnectionOptions opt)
            {
                opt.Headers = new Dictionary<string, string>
                    {
                        // "username" parameter could also be email, but we have to use one or other consistently for 
                        // the websocket connection to be grouped correctly by username serverside
                        { "Username" , _userData.User.Username },
                        { "Authorization" , tokenValue }
                    };
            }
            //TimeSpan[] timeouts = { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2) };
            DataHubConnection = new HubConnectionBuilder()
                .WithUrl($"{APIBaseUrl()}/DataHub", websocketOptions)
                //.WithAutomaticReconnect(timeouts)
                .Build();
        }

        public override async Task RegisterClientDevice()
        {
            string device = Environment.MachineName;
            string platform = Environment.OSVersion.VersionString;

            ClientDTO clientDTO = new ClientDTO(device, platform);
            string json = JsonSerializer.Serialize(clientDTO);
            var body = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage re = await httpClient.PostAsync("/api/client", body);
            } catch
            {
                // not a huge issue, just do nothing
            }
        }

        public override async Task<AuthenticationResult> AuthenticateUser(string username, string password)
        {
            UserPWParams pwParams = await GetUserPasswordSalt(username);

            string passwordHash = RFC2898Helper.ComputePasswordHash(password, pwParams.PasswordSalt, 
                pwParams.AlgorithmIdentifier.Iterations, pwParams.AlgorithmIdentifier.HashLen);

            AuthenticatingUser authUser = new AuthenticatingUser(username, passwordHash);
            (bool authResult, dynamic resultObj) = await Authenticate(authUser);

            if (authResult == false)
            {
                string msg = resultObj is HttpResponseMessage re ? re.ReasonPhrase : resultObj.ToString();
                return new AuthenticationResult(false, resultObj, msg, null);
            }

            string token = resultObj.access_Token;

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            User user = null;

            using HttpResponseMessage response = await GetAsync("/api/user");
            if (response.IsSuccessStatusCode)
            {
                user = await response.Content.ReadAsAsync<User>();
                _userData.SetUserData(user, password);
                string responseStr = await response.Content.ReadAsStringAsync();
                return new AuthenticationResult(true, response, responseStr, user);
            }
            else
            {
                return new AuthenticationResult(false, response, response.ReasonPhrase, user);
            }
        }

        private async Task<(bool, dynamic)> Authenticate(AuthenticatingUser user)
        {
            string jsonData = JsonSerializer.Serialize(user);
            var body = new StringContent(jsonData, Encoding.UTF8, "application/json");

            using HttpResponseMessage response = await PostAsync("/api/token", body);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<dynamic>();
                return (true, result);
            }
            else
            {
                try
                {
                    string errmsg = await response.Content.ReadAsStringAsync();
                    return (false, errmsg);
                }
                catch (Exception)
                {
                    return (false, response);
                }
            }
        }

        private async Task<UserPWParams> GetUserPasswordSalt(string username)
        {
            using HttpResponseMessage response = await httpClient.GetAsync($"/api/salt/{username}");

            if (response.IsSuccessStatusCode)
            {
                UserPWParams pwParams = await response.Content.ReadAsAsync<UserPWParams>();
                return pwParams;
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

        public override Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            return httpClient.GetAsync(requestUri);
        }

        public override Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent body)
        {
            return httpClient.PostAsync(requestUri, body);
        }

        public override Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent body)
        {
            return httpClient.PutAsync(requestUri, body);
        }

        public override Task<HttpResponseMessage> DeleteAsync(string requestUri)
        {
            return httpClient.DeleteAsync(requestUri);
        }

        public static async Task<(HttpResponseMessage, string)> RegisterUser(UserDTO registeringUser)
        {
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(APIBaseUrl())
            };
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string jsonData = JsonSerializer.Serialize(registeringUser);
            var body = new StringContent(jsonData, Encoding.UTF8, "application/json");

            using HttpResponseMessage response = await client.PostAsync("/api/register", body);
            if (response.IsSuccessStatusCode)
                return (response, "OK");
            else return (response, await response.Content.ReadAsStringAsync());
        }

        public static async Task<(HttpResponseMessage, string)> PostFeedback(FeedbackDTO feedback)
        {
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(APIBaseUrl())
            };
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string jsonData = JsonSerializer.Serialize(feedback);
            var body = new StringContent(jsonData, Encoding.UTF8, "application/json");

            using HttpResponseMessage response = await client.PostAsync("/api/feedback", body);
            if (response.IsSuccessStatusCode)
                return (response, "OK");
            else return (response, await response.Content.ReadAsStringAsync());
        }
    }
}
