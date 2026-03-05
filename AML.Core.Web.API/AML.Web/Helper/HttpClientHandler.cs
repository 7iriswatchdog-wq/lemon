using AML.Core.Common.StaticResource;
using AML.Core.DataContract.Authentication;
using AML.Core.ServiceContract.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AML.Web.Helper
{
    public interface IHttpClientHandler
    {
        int GetUserId();
        int GetGroupId();
        int GetBranchId();
        int GetClientId();
        string Decrypt(object value = null);
        public string Encrypt(object value = null);
        void SetStringSession(string key, string value);
        void SetObjectSession(string key, object value);
        public T GetSessionObject<T>(string key);

        Task<dynamic> PostAsync(dynamic model, string url);
        //Task<dynamic> GetAsync(dynamic model, string url);
        Task<dynamic> GetAsync(TokenRS model, string url);
    }
    public class HttpClientHandler : IHttpClientHandler
    {
        private readonly IConfiguration configuration;
        private ICommonService commonService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private string baseURL = string.Empty;
        public HttpClientHandler(IConfiguration _configuration, ICommonService _commonService, IHttpContextAccessor _httpContextAccessor)
        {
            configuration = _configuration;
            commonService = _commonService;
            httpContextAccessor = _httpContextAccessor;
            baseURL = configuration.GetSection("AMLBaseApiUrl").GetSection("BaseUrl").Value;
        }
        public int GetUserId()
        {
            return string.IsNullOrEmpty(httpContextAccessor.HttpContext.Session.GetString(StaticResource.sessUserId)) ? 0 : Convert.ToInt32(httpContextAccessor.HttpContext.Session.GetString(StaticResource.sessUserId));
        }
        public int GetGroupId()
        {
            return string.IsNullOrEmpty(httpContextAccessor.HttpContext.Session.GetString(StaticResource.sessRoleId)) ? 0 : Convert.ToInt32(httpContextAccessor.HttpContext.Session.GetString(StaticResource.sessRoleId));
        }
        public int GetBranchId()
        {
            return string.IsNullOrEmpty(httpContextAccessor.HttpContext.Session.GetString(StaticResource.sessBranchId)) ? 0 : Convert.ToInt32(httpContextAccessor.HttpContext.Session.GetString(StaticResource.sessBranchId));
        }

        public int GetClientId()
        {
            return string.IsNullOrEmpty(httpContextAccessor.HttpContext.Session.GetString(StaticResource.sessClientId)) ? 0 : Convert.ToInt32(httpContextAccessor.HttpContext.Session.GetString(StaticResource.sessClientId));
        }
        public string Decrypt(object value = null)
        {
            if (!string.IsNullOrEmpty(value.ParseString()))
            {
                return commonService.DecryptionString(value.ParseString());
            }
            else
            {
                return string.Empty;
            }
        }
        public string Encrypt(object value = null)
        {
            if (!string.IsNullOrEmpty(value.ParseString()))
            {
                return commonService.EncryptString(value.ParseString());
            }
            else
            {
                return string.Empty;
            }
        }
        public void SetStringSession(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
                httpContextAccessor.HttpContext.Session.SetString(key, value);
        }
        public string GetSessionString(string key)
        {
            return httpContextAccessor.HttpContext.Session.GetString(key);
        }
        public void SetObjectSession(string key, object value)
        {
            httpContextAccessor.HttpContext.Session.SetObjectAsJson(key, value);
        }
        public T GetSessionObject<T>(string key)
        {
            return httpContextAccessor.HttpContext.Session.GetObjectFromJson<T>(key);
        }



        public async Task<dynamic> GetAsync(TokenRS model, string url)
        {
            string result = String.Empty;
            dynamic modelResponse = model;
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            #region GET REQUEST
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders
           .Accept
           .Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", model.user.token);
                //Add Token Headers            
                try
                {
                    //client.DefaultRequestHeaders.Add("authUser", model.user.id);
                    //client.DefaultRequestHeaders.Add("authCompany", model.user.username);
                    //client.DefaultRequestHeaders.Add("authKey", model.user.token);
                }
                catch { }
                // client.BaseAddress = new Uri(baseURL);
                client.BaseAddress = new Uri(url);
                //HttpResponseMessage response = await client.GetAsync(client.BaseAddress + url);
                HttpResponseMessage response = await client.GetAsync(client.BaseAddress);
                
                if (response.IsSuccessStatusCode)
                {
                     result = await response.Content.ReadAsStringAsync();
                    modelResponse = JsonConvert.DeserializeObject<dynamic>(result);
                }
            }
            #endregion

            return result;
        }

        /// <summary>
        /// HttpClient Post Request
        /// </summary>
        /// <param name="model"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<dynamic> PostAsync(dynamic model, string url)
        {
            dynamic result = string.Empty;
            #region POST Content Setter
            string postContent = JsonConvert.SerializeObject(model);
            var buffer = Encoding.UTF8.GetBytes(postContent);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            #endregion
            #region POST REQUEST
            using (HttpClient client = new HttpClient())
            {
                //Add Token Headers            
                try
                {
                    client.DefaultRequestHeaders.Add("authUser", model.AuthToken.UserId);
                    client.DefaultRequestHeaders.Add("authCompany", model.AuthToken.CompanyId);
                    client.DefaultRequestHeaders.Add("authKey", model.AuthToken.AuthKey);
                }
                catch(Exception ex) {
                    var error = "error";
                }
                client.BaseAddress = new Uri(baseURL);
                Console.WriteLine(baseURL + url);
                HttpResponseMessage response = await client.PostAsync(baseURL + url, byteContent);
                Console.WriteLine(response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    result = await response.Content.ReadAsStringAsync();
                }
            }
            #endregion
            return result;
        }
    }
}
