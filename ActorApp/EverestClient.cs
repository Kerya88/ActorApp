using Akka.Configuration.Hocon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection.Emit;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ActorApp
{
    public class EverestClient
    {
        private readonly HttpClient _httpClient;

        public EverestClient(string token)
        {
            var handler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer()
            };
            handler.CookieContainer.Add(new Uri("https://everest.distcomp.org"), new Cookie("access_token", "qobfc6tzfpbatax5q8jevcva6cstn1ctm62k7fzdkfgz4qnlizyj6a950yt06aat"));
            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://everest.distcomp.org/api")
            };
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }

        public async Task<string> GetToken(string username, string password, string label)
        {
            var jsonData = JsonSerializer.Serialize(new { username, password, label });
            var request = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"/auth/access_token", request);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);
            var token = json!["access_token"].ToString()!;

            return token;
        }

        public async Task RunJob(string name, string[] resources, Dictionary<string, object> inputs, string appId)
        {
            var jsonData = JsonSerializer.Serialize(new { name, inputs, resources });
            var request = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"/apps/{appId}", request);

            var ss = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody);
        }

        public async Task CheckState(string jodId)
        {
            var response = await _httpClient.GetAsync($"/jobs/{jodId}");
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody);
        }
    }
}
