using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ActorApp
{
    public class EverestClient
    {
        //http клиент
        private readonly HttpClient _httpClient;
        //базовый адрес
        private readonly Uri _baseUri =  new ("https://everest.distcomp.org");

        public EverestClient(string token)
        {
            //http обработчик
            var httpHandler = new HttpClientHandler
            {
                //использовать куки
                UseCookies = true,
                //контейнер куки
                CookieContainer = new CookieContainer(),
                //обработка сертификата сервера
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            //добавление куки с токеном
            httpHandler.CookieContainer.Add(_baseUri, new Cookie("access_token", token)
            {
                //распространяется на весь домен
                Path = "/",
                //не секретный
                Secure = false,
                //не только для http
                HttpOnly = false,
                //не истекает
                Expires = DateTime.MinValue
            });
            
            _httpClient = new HttpClient(httpHandler);
            _httpClient.BaseAddress = _baseUri;
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("charsets", "utf-8");
        }
        
        //запуск работы
        public string RunJob(string name, string[] resources, Dictionary<string, object> inputs, string appId)
        {
            //сериализуем данные запроса
            var jsonData = JsonSerializer.Serialize(new { name, inputs, resources });
            var request = new StringContent(jsonData, Encoding.UTF8, "application/json");
            
            var response = _httpClient.PostAsync($"/api/apps/{appId}", request).Result;
                
            response.EnsureSuccessStatusCode();
            
            var responseBody = response.Content.ReadAsStringAsync().Result;
            var json = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);
            var jobId = json!["id"].ToString()!;
            
            return jobId;
        }

        //проверка статуса работы
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
        
        private class InnerResult
        {
            public string partialSum { get; set; }
        }
    }
}
