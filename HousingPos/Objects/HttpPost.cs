using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http;
using System.Security.Cryptography;
using Dalamud.Plugin;

namespace HousingPos.Objects
{
    
    public class CloudMap
    {
        public static CloudMap Empty => new CloudMap(0, "", "", "", "");

        public int LocationId;
        public string Name;
        public string Hash;
        public string ObjectId;
        public string Tags;
        public CloudMap(int locationId,  string uploadName, string hash, string tags, string objectId)
        {
            LocationId = locationId;
            Name = uploadName;
            Hash = hash;
            Tags = tags;
            ObjectId = objectId;
        }
    }
    
    public class HttpPost
    {
        // private static string appId = "OHAlmaVE5wP7gXT4dDpcpqsv-MdYXbMMI";
        // private static string appkey = "XkMdaB5RAXIeCOGX1NUL7FIj";
        public static string GetMD5(string SourceData, string salt)
        {
            byte[] tmpData;
            byte[] tmpHash;
            tmpData = ASCIIEncoding.ASCII.GetBytes(SourceData + salt);
            tmpHash = new MD5CryptoServiceProvider().ComputeHash(tmpData);
            StringBuilder sOutput = new StringBuilder(tmpHash.Length);
            for (int i = 0; i < tmpHash.Length; i++)
            {
                sOutput.Append(tmpHash[i].ToString("X2"));
            }
            return sOutput.ToString();
        }
        public static async Task<string> Post(string Uri, int LocationId, string UploadName, string str, string tags, string Uploader, string UserId, string Md5Salt)
        {
            if (str == null || str == "" || str == "[]")
                return "You Can't Upload An Empty List.";
            HttpClient httpClient = new HttpClient();
            var UserHash = GetMD5(UserId, Md5Salt);
            var values = new Dictionary<string, string>
            {
                {"LocationId", LocationId.ToString()},
                {"UploadName", UploadName },
                {"Items", str },
                {"Tags", tags },
                {"Uploader", Uploader },
                {"UserId",UserHash }
            };
            HttpContent data = new FormUrlEncodedContent(values);
            HttpResponseMessage response = await httpClient.PostAsync(Uri + "/index.php", data);
            response.EnsureSuccessStatusCode();
            string resultStr = await response.Content.ReadAsStringAsync();
            return resultStr;
        }
        /*
        public static async Task<string> Login(string Uri, string UserName)
        {
            HttpClient httpClient = new HttpClient();
            if (UserName == ""){
                return "";
            }
            var UserHash = GetMD5(UserName);
            var values = new Dictionary<string, string>
            {
                {"username", UserName},
                {"password", UserHash}
            };
            var json = JsonConvert.SerializeObject(values);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Add("X-LC-Id", appId);
            httpClient.DefaultRequestHeaders.Add("X-LC-Key", appkey);
            HttpResponseMessage response = await httpClient.PostAsync(Uri+"/users", data);
            if (!response.IsSuccessStatusCode)
            {
                string resultStr = await response.Content.ReadAsStringAsync();
                HttpClient httpClient2 = new HttpClient();
                httpClient2.DefaultRequestHeaders.Add("X-LC-Id", appId);
                httpClient2.DefaultRequestHeaders.Add("X-LC-Key", appkey);
                var json2 = JsonConvert.SerializeObject(values);
                var data2 = new StringContent(json2, Encoding.UTF8, "application/json");
                HttpResponseMessage response2 = await httpClient2.PostAsync(Uri + "/login", data2);
                response2.EnsureSuccessStatusCode();
                string resultStr2 = await response2.Content.ReadAsStringAsync();
                return resultStr2;
            }
            else
            {
                string resultStr = await response.Content.ReadAsStringAsync();
                return resultStr;
            }
        }
        public static async Task<string> PostWithLeanCloud(string Uri,string ClassName, string Location, string UploadName, string str, string tags, string Uploader,string sessionToken)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("X-LC-Id", appId);
            httpClient.DefaultRequestHeaders.Add("X-LC-Key", appkey);
            httpClient.DefaultRequestHeaders.Add("X-LC-Session", sessionToken);
            if (str == null)
            {
                return "You Can't Upload An Empty List.";
            }
            var values = new Dictionary<string, string>
            {
                {"Location", Location},
                {"UploadName", UploadName },
                {"Items", str },
                {"Tags", tags },
                {"Uploader", Uploader },
                {"ItemHash",GetMD5(str) }
            };
            var json = JsonConvert.SerializeObject(values);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            data.Headers.ContentType.MediaType = "application/json";
            HttpResponseMessage response = await httpClient.PostAsync(Uri + ClassName, data);
            response.EnsureSuccessStatusCode();
            string resultStr = await response.Content.ReadAsStringAsync();
            return resultStr;
        }
        */
        public static async Task<string> GetMap(string Uri)
        {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync(Uri + "/map.json");
            response.EnsureSuccessStatusCode();
            string resultStr = await response.Content.ReadAsStringAsync();
            return resultStr;
        }
        /*
        public static async Task<string> GetMapWithLeanCloud(string Uri)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("X-LC-Id", appId);
            httpClient.DefaultRequestHeaders.Add("X-LC-Key", appkey);
            HttpResponseMessage response = await httpClient.GetAsync(Uri + "?keys=Location,Tags,UploadName,-createdAt,-updatedAt");
            response.EnsureSuccessStatusCode();
            string resultStr = await response.Content.ReadAsStringAsync();
            return resultStr;
        }
        */
        public static async Task<string> GetItems(string Uri, string hash)
        {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync(Uri + "/result/" + hash + ".json");
            response.EnsureSuccessStatusCode();
            string resultStr = await response.Content.ReadAsStringAsync();
            return resultStr;
        }
        /*
        public static async Task<string> GetItemsWithLeanCloud(string Uri, string hash)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("X-LC-Id", appId);
            httpClient.DefaultRequestHeaders.Add("X-LC-Key", appkey);
            HttpResponseMessage response = await httpClient.GetAsync(Uri +"/"+ hash + "?keys=Items,-objectId,-updatedAt,-createdAt");
            response.EnsureSuccessStatusCode();
            string resultStr = await response.Content.ReadAsStringAsync();
            return resultStr;
        }
        public static async Task<string> DelItemsWithLeanCloud(string Uri, string hash, string sessionToken)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("X-LC-Id", appId);
            httpClient.DefaultRequestHeaders.Add("X-LC-Key", appkey);
            httpClient.DefaultRequestHeaders.Add("X-LC-Session", sessionToken);
            HttpResponseMessage response = await httpClient.GetAsync(Uri + "/" + hash + "?keys=Items,-objectId,-updatedAt,-createdAt");
            response.EnsureSuccessStatusCode();
            string resultStr = await response.Content.ReadAsStringAsync();
            return resultStr;
        }
        */
    }
}
