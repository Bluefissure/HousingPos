using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Dalamud.Plugin;

namespace HousingPos.Objects
{
    public class CloudMap
    {
        public static CloudMap Empty => new CloudMap("", "", "");

        public string Name;
        public string Hash;
        public string Tags;
        public CloudMap(string named, string hash, string tags)
        {
            Name = named;
            Hash = hash;
            Tags = tags;
        }
    }
    
    public class HttpPost
    {
        public static async Task<string> Post(string Uri,string Location, string Size, string Nameit, string str,string tags,string uper)
        {
            HttpClient httpClient = new HttpClient();
            var values = new Dictionary<string, string>
            {
                {"Location",Location},
                {"Size",Size },
                {"Named",Nameit },
                {"Items",str },
                {"Tags",tags },
                {"Uper",uper }
            };
            HttpContent data = new FormUrlEncodedContent(values);
            //创建一个异步HTTP请求，当请求返回时继续处理
            HttpResponseMessage response = await httpClient.PostAsync(Uri + "/index.php", data);
            response.EnsureSuccessStatusCode();
            string resultStr = await response.Content.ReadAsStringAsync();
            return resultStr;
        }
        public static async Task<string> GetMap(string Uri)
        {
            HttpClient httpClient = new HttpClient();
            //创建一个异步HTTP请求，当请求返回时继续处理
            HttpResponseMessage response = await httpClient.GetAsync(Uri + "/map.json");
            response.EnsureSuccessStatusCode();
            string resultStr = await response.Content.ReadAsStringAsync();
            return resultStr;
        }
        public static async Task<string> GetItems(string Uri, string hash)
        {
            HttpClient httpClient = new HttpClient();
            //创建一个异步HTTP请求，当请求返回时继续处理
            HttpResponseMessage response = await httpClient.GetAsync(Uri + "/result/" + hash + ".json");
            response.EnsureSuccessStatusCode();
            string resultStr = await response.Content.ReadAsStringAsync();
            return resultStr;
        }
    }
}
