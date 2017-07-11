using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PWTransfer.Tests
{
    public static class HttpClientExtensions
    {
        public static async Task<long> ToLong(this Task<string> response)
        {
            return long.Parse(await response);
        }

        public static async Task<T> Content<T>(this Task<HttpResponseMessage> response)
        {
            return JsonConvert.DeserializeObject<T>(await response.Content());
        }

        public static async Task<string> Content(this Task<HttpResponseMessage> response)
        {
            return await (await response).Content.ReadAsStringAsync();
        }

        public static async Task<HttpResponseMessage> PostAsync(this HttpClient client, string route, object body)
        {
            var content = JsonConvert.SerializeObject(body);
            var buffer = System.Text.Encoding.UTF8.GetBytes(content);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return await client.PostAsync(route, byteContent);
        }

        public static async Task<HttpResponseMessage> PostFormAsync(this HttpClient client, string route, Dictionary<string, string> formData)
        {
            var content = new FormUrlEncodedContent(formData);
            return await client.PostAsync(route, content);
        }
    }
}
