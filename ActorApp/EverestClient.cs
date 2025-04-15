using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ActorApp
{
    public class EverestClient
    {
        private readonly HttpClient _httpClient;
        private readonly Uri _baseUri =  new ("https://everest.distcomp.org");

        public EverestClient(string token)
        {
            var httpHandler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new CookieContainer(),
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            httpHandler.CookieContainer.Add(_baseUri, new Cookie("access_token", token)
            {
                Path = "/",
                Secure = false,
                HttpOnly = false,
                Expires = DateTime.MinValue
            });
            
            _httpClient = new HttpClient(httpHandler);
            _httpClient.BaseAddress = _baseUri;
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("charsets", "utf-8");
            
            var testResponse = _httpClient.GetAsync("/api/jobs/0").Result;
            
            if (testResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                token = GetToken("Kerya88", "RUZ365gar31", "ActorApp");
                    
                httpHandler.CookieContainer.Add(_baseUri, new Cookie("access_token", token)
                {
                    Path = "/",
                    Secure = false,
                    HttpOnly = false,
                    Expires = DateTime.MinValue
                });
            }
        }
        
        public string RunJob(string name, string[] resources, Dictionary<string, object> inputs, string appId)
        {
            var jsonData = JsonSerializer.Serialize(new { name, inputs, resources });
            var request = new StringContent(jsonData, Encoding.UTF8, "application/json");
            
            var response = _httpClient.PostAsync($"/api/apps/{appId}", request).Result;
                
            response.EnsureSuccessStatusCode();
            
            var responseBody = response.Content.ReadAsStringAsync().Result;
            var json = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);
            var jobId = json!["id"].ToString()!;
            
            return jobId;
        }

        public (bool, string) CheckState(string jodId)
        {
            var response = _httpClient.GetAsync($"/api/jobs/{jodId}").Result;
            
            response.EnsureSuccessStatusCode();

            var responseBody = response.Content.ReadAsStringAsync().Result;
            var json = JsonSerializer.Deserialize<Dictionary<string,object>>(responseBody)!;
            
            var deleteResult = _httpClient.DeleteAsync($"/api/jobs/{jodId}").Result;

            switch (json["state"].ToString()!)
            {
                case "DONE":
                {
                    return (true, JsonSerializer.Deserialize<InnerResult>(JsonSerializer.Serialize(json["result"])).partialSum.Replace("\n", ""));
                }
                case "FAILED":
                case "CANCELLED":
                {
                    return (true, json["info"].ToString()!);
                }
                default:
                {
                    return (true, "");
                }
            }
        }
        
        private string GetToken(string username, string password, string label)
        {
            var jsonData = JsonSerializer.Serialize(new { username, password, label });
            var request = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var response = _httpClient.PostAsync($"/auth/access_token", request).Result;
            response.EnsureSuccessStatusCode();

            var responseBody = response.Content.ReadAsStringAsync().Result;
            var json = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);
            var token = json!["access_token"].ToString()!;

            return token;
        }

        private class OuterResult
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string AppId { get; set; }
            public string AppAlias { get; set; }
            public string AppName { get; set; }
            public string AppVersion { get; set; }
            public string User { get; set; }
            public string State { get; set; }
            public int Submitted { get; set; }
            public Inputs Inputs { get; set; }
            public object ResourceRequirments { get; set; }
            public string[] Resources { get; set; }
            public string[] AllowList { get; set; }
            public int LastUpdateTime { get; set; }
            public string Description { get; set; }
            public bool NotifyUser { get; set; }
            public int DataSize { get; set; }
            public string Info { get; set; }
            public InnerResult Result { get; set; }
        }
        private class Inputs
        {
            public string S { get; set; }
            public string F { get; set; }
        }
        
        private class InnerResult
        {
            public string partialSum { get; set; }
        }
    }
}
