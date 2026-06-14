using AML.Core.ServiceContract.Sanction;
using AML.DTO.DTO.CustomerScreening;
using AML.DTO.DTO.Sanction;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AML.Core.Service.Sanction
{
    public class ScreeningService: IScreeningService
    {
        private string baseURL = string.Empty;
        private readonly IConfiguration configuration;

        public ScreeningService(IConfiguration _configuration)
        {
            configuration = _configuration;
            baseURL = configuration.GetSection("AMLBaseApiUrl").GetSection("BaseUrl").Value;
        }
        public CustomerScreeningRS BlackListSearch(ScreeningSearchDTO model)
        {
            CustomerScreeningRS apiResp = new CustomerScreeningRS();
            try
            {

                //Calling Screening API
                var response = PostAsync(new
                {
                    customerfullname = model.customerfullname,
                    searchtype = model.searchtype,
                    customernationality = model.customernationality,
                    customerdob = model.customerdob
                }, AML.Core.Common.StaticResource.ScreeningService.BACKLIST_SCREENING);
                if (response.Result != null)
                    apiResp = JsonConvert.DeserializeObject<CustomerScreeningRS>(response.Result);
            }
            catch { }

            return apiResp;
        }

        private async Task<dynamic> PostAsync(dynamic model, string url)
        {
            dynamic result = string.Empty;
            #region POST Content Setter
            string postContent = JsonConvert.SerializeObject(model);
            var buffer = Encoding.UTF8.GetBytes(postContent);
            var byteContent = new ByteArrayContent(buffer);

            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            #endregion
            #region POST REQUEST
            using (HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
            {
                var content = new StringContent(model.ToString(), Encoding.UTF8, "application/json");
                client.BaseAddress = new Uri(baseURL);
                HttpResponseMessage response = await client.PostAsync(baseURL + url, content);
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
