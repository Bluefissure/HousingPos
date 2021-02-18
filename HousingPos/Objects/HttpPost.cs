using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Dalamud.Plugin;

namespace HousingPos.Objects
{
    public class HttpPost
    {
        private const string Uri = "https://api.4c43.work/ffxiv/index.php";
        public static async Task<string> Post(string Location, string Nameit, string str,string tags,string uper)
        {
            HttpClient httpClient = new HttpClient();
            var values = new Dictionary<string, string>
            {
                {"Location",Location},
                {"Named",Nameit },
                {"Items",str },
                {"Tags",tags },
                {"Uper",uper }
            };
            HttpContent data = new FormUrlEncodedContent(values);
            Console.WriteLine(str);
            Console.WriteLine(data);
            //创建一个异步HTTP请求，当请求返回时继续处理
            HttpResponseMessage response = await httpClient.PostAsync(Uri, data);
            response.EnsureSuccessStatusCode();
            string resultStr = await response.Content.ReadAsStringAsync();
            return resultStr;
        }
    }
}
