using MiniLaunch.Common.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace MiniLaunch.Common
{
    public class SoapClient : IDisposable
    {
        private readonly HttpClient _httpClient;

        public SoapClient(bool preview = false)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(preview ? "https://gls-lm.ddo.com" : "https://gls.ddo.com", UriKind.Absolute)
            };
        }

        private Task<HttpResponseMessage> MakeSoapRequest(string uri, string soapRequestBody, HttpMethod httpMethod)
        {
            var requestHeader = "<?xml version=\"1.0\" encoding=\"utf-8\"?><soap12:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap12=\"http://www.w3.org/2003/05/soap-envelope\"><soap12:Body>";
            var requestFooter = "</soap12:Body></soap12:Envelope>";

            var requestMsg = new HttpRequestMessage(httpMethod, new Uri(uri, UriKind.Relative));

            if (!string.IsNullOrWhiteSpace(soapRequestBody))
            {
                requestMsg.Content = new StringContent(requestHeader + soapRequestBody + requestFooter, Encoding.UTF8, "text/xml");
            }

            return _httpClient.SendAsync(requestMsg);
        }

        private T DeserializeResponse<T>(string responseContent)
        {
            const string soapHeader = "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><soap:Body>";
            const string soapFooter = "</soap:Body></soap:Envelope>";

            var trimmedResponse = responseContent.Replace(soapHeader, "").Replace(soapFooter, "");

            var serializer = new XmlSerializer(typeof(T));

            T deserializedXml;

            using (var reader = new StringReader(trimmedResponse))
            {
                deserializedXml = (T)serializer.Deserialize(XmlReader.Create(reader));
            }

            return deserializedXml;
        }

        public async Task<LoginAccountResponse> LoginAccount(string username, string password)
        {
            const string requestUri = "/GLS.AuthServer/Service.asmx";
            var requestBody = "<LoginAccount xmlns=\"http://www.turbine.com/SE/GLS\"><username>" + username + "</username><password>" + password + "</password></LoginAccount>";

            var response = await MakeSoapRequest(requestUri, requestBody, HttpMethod.Post);
            var responseString = await response.Content.ReadAsStringAsync();

            return DeserializeResponse<LoginAccountResponse>(responseString);
        }

        public async Task<GetDatacentersResponse> GetDatacenters(string game)
        {
            const string requestUri = "/GLS.DataCenterServer/Service.asmx";
            var requestBody = "<GetDatacenters xmlns=\"http://www.turbine.com/SE/GLS\"><game>" + game + "</game></GetDatacenters>";

            var response = await MakeSoapRequest(requestUri, requestBody, HttpMethod.Post);
            var responseString = await response.Content.ReadAsStringAsync();

            return DeserializeResponse<GetDatacentersResponse>(responseString);
        }

        public async Task<Status> GetDatacenterStatus(string serverIp)
        {
            const string requestUri = "/GLS.DataCenterServer/StatusServer.aspx?s=";

            var response = await MakeSoapRequest(requestUri + serverIp, null, HttpMethod.Get);
            var responseString = await response.Content.ReadAsStringAsync();

            return DeserializeResponse<Status>(responseString);
        }

        public async Task<Result> QueueTakeANumber(string subscription, string ticket, string queueUrl)
        {
            const string requestUri = "/GLS.AuthServer/LoginQueue.aspx";
            var requestBody = "command=TakeANumber&subscription=" + subscription + "&ticket=" + HttpUtility.UrlEncode(ticket) + "&ticket_type=GLS&queue_url=" + queueUrl;

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(requestUri, UriKind.Relative))
            {
                Content = new StringContent(requestBody, Encoding.UTF8, "application/x-www-form-urlencoded")
            };

            var response = await _httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            return DeserializeResponse<Result>(responseString);
        }

        public async Task<Dictionary<string, string>> GetLauncherConfig()
        {
            const string requestUri = "/launcher/ddo/dndlauncher.server.config.xml";

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