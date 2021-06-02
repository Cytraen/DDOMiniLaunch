using MiniLaunch.Common.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MiniLaunch.Common
{
    public class SoapClient : IDisposable
    {
        private readonly HttpClient _httpClient;

        public SoapClient()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://gls.ddo.com", UriKind.Absolute)
            };
        }

        private Task<HttpResponseMessage> MakeSoapRequest(string uri, string soapRequestBody, HttpMethod httpMethod)
        {
            var requestHeader = "<?xml version=\"1.0\" encoding=\"utf-8\"?><soap12:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap12=\"http://www.w3.org/2003/05/soap-envelope\"><soap12:Body>;";
            var requestFooter = "</soap12:Body></soap12:Envelope>";

            var requestMsg = new HttpRequestMessage(httpMethod, new Uri(uri, UriKind.Relative));

            if (!string.IsNullOrWhiteSpace(soapRequestBody))
            {
                requestMsg.Content = new StringContent(requestHeader + soapRequestBody + requestFooter, Encoding.UTF8, "text /xml");
            }

            return _httpClient.SendAsync(requestMsg);
        }

        private T DeserializeResponse<T>(string responseContent)
        {
            const string xmlHeader = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            const string soapHeader = "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><soap:Body>";
            const string soapFooter = "</soap:Body></soap:Envelope>";

            var trimmedResponse = responseContent.Replace(xmlHeader, "").Replace(soapHeader, "").Replace(soapFooter, "");
            return JsonSerializer.Deserialize<T>(trimmedResponse);
        }

        private async Task<LoginAccountResponse> LoginAccount(string username, string password)
        {
            var requestUri = "/GLS.AuthServer/Service.asmx";
            var requestBody = "<LoginAccount xmlns=\"http://www.turbine.com/SE/GLS\"><username>" + username + "</username><password>" + password + "</password></LoginAccount>";

            var response = await MakeSoapRequest(requestUri, requestBody, HttpMethod.Post);
            var responseString = await response.Content.ReadAsStringAsync();

            return DeserializeResponse<LoginAccountResponse>(responseString);
        }

        private async Task<GetDatacentersResponse> GetDatacenters(string game)
        {
            var requestUri = "/GLS.DataCenterServer/Service.asmx";
            var requestBody = "<GetDatacenters xmlns=\"http://www.turbine.com/SE/GLS\"><game>" + game + "</game></GetDatacenters>";

            var response = await MakeSoapRequest(requestUri, requestBody, HttpMethod.Post);
            var responseString = await response.Content.ReadAsStringAsync();

            return DeserializeResponse<GetDatacentersResponse>(responseString);
        }

        private async Task<Dictionary<string, string>> GetLauncherConfig()
        {
            var requestUri = "/launcher/ddo/dndlauncher.server.config.xml";

            var response = await MakeSoapRequest(requestUri, null, HttpMethod.Get);
            var responseString = await response.Content.ReadAsStringAsync();

            var configXml = DeserializeResponse<configuration>(responseString);

            return configXml.appSettings.ToDictionary(x => x.key, x => x.value);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}