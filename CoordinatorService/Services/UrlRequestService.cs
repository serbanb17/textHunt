using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CoordinatorService.Services
{
    public class UrlRequestService
    {
        private static async Task<Tuple<bool, string>> GetRequestAsync(string url)
        {
            bool isOk = false;
            string result = null;
            try
            {
                var httpClient = HttpClientFactory.Create();
                HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(url);
                isOk = httpResponseMessage.StatusCode == HttpStatusCode.OK;
                if (isOk)
                {
                    var content = httpResponseMessage.Content;
                    result = await content.ReadAsStringAsync();
                }
            }
            catch
            {
                isOk = false;
                result = null;
            }

            return new Tuple<bool, string>(isOk, result);
        }
        private static async Task<Tuple<bool, string>> PostRequestAsync(string url, object reqBody)
        {
            bool isOk = false;
            string result = null;
            try
            {
                var reqBodyJson = new StringContent(reqBody is null ? "" : JsonSerializer.Serialize(reqBody), Encoding.UTF8, "application/json");
                var httpClient = HttpClientFactory.Create();
                HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(url, reqBodyJson);
                isOk = httpResponseMessage.StatusCode == HttpStatusCode.OK;
                if (isOk)
                {
                    var content = httpResponseMessage.Content;
                    result = await content.ReadAsStringAsync();
                }
            }
            catch
            {
                isOk = false;
                result = null;
            }

            return new Tuple<bool, string>(isOk, result);
        }
        public static bool GetRequest(string url, out string result)
        {
            var reqTask = GetRequestAsync(url);
            reqTask.Wait();
            result = reqTask.Result.Item2;
            return reqTask.Result.Item1;
        }
        public static bool PostRequest(string url, object reqBody, out string result)
        {
            var reqTask = PostRequestAsync(url, reqBody);
            reqTask.Wait();
            result = reqTask.Result.Item2;
            return reqTask.Result.Item1;
        }
    }
}