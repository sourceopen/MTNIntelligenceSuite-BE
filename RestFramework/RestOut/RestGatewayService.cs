using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace NavisSmartRestGateway
{
    /// <summary>
    /// Helper class to deal with http requests to Navis Smart Data API (REST)
    /// </summary>
    public class RestGatewaySevice
    {
        private readonly static string reservedCharacters = "!*'();:@&=+$,/?%#[]";
        public string WebservicePath { get; set; }
        public string WebserviceName { get; set; }

        /// <summary>
        /// Builds a request message for every request.
        /// </summary>
        /// <param name="httpMethod">GET/PUT</param>
        /// <param name="methodName">Actual REST method name for Navis Smart Data API</param>
        /// <param name="queryParams">params to pass to the REST API</param>
        /// <param name="authToken">internal microservice token</param>
        /// <returns></returns>
        public HttpRequestMessage BuildRequestMessage(HttpMethod httpMethod, string methodName, Dictionary<string, string> queryParams, string authToken)
        {
            var message = new HttpRequestMessage(httpMethod, BuildRequestPath(methodName, queryParams));
            message.Headers.Add(ServiceConstants.MicroserviceHeaderKey, ServiceConstants.MicroserviceHeaderValue);
            message.Headers.Authorization = new AuthenticationHeaderValue("Basic", authToken);
            return message;
        }

        public StringContent ConvertToJson(object o)
        {
            return new StringContent(JsonConvert.SerializeObject(o), Encoding.UTF8, "application/json");
        }

        public string UrlEncode(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            var sb = new StringBuilder();

            foreach (char @char in value)
            {
                if ((reservedCharacters.IndexOf(@char) == -1) && (@char.ToString() != @"\"))
                {
                    sb.Append(@char);
                }
                else
                {
                    if (@char.ToString() == @"+")
                    {
                        sb.Append("%20PLUS%20");
                    }
                    else
                    {
                        sb.AppendFormat("%{0:X2}", (int)@char);
                    }
                }
            }
            return sb.ToString();
        }

        private string BuildRequestPath(string methodName, Dictionary<string, string> queryParams)
        {
            StringBuilder requestPath = new StringBuilder();

            requestPath.AppendFormat("{0}/{1}?{2}", WebservicePath, methodName, WebserviceName);
            if (queryParams != null && queryParams.Any())
            {
                foreach (KeyValuePair<string, string> entry in queryParams)
                {
                    requestPath.AppendFormat("&{0}={1}", entry.Key, entry.Value);
                }
            }

            return requestPath.ToString();
        }
    }

    public class MethodNames
    {
        //all method names can go here as constants
        public const string MicroserviceToken = "MicroserviceToken";

        public const string SystemInfo = "Info";
        public const string SystemData = "SystemData";
        public const string ComplexInfo = "ComplexInfo";
        public const string TerminalData = "TerminalData";
        public const string ProducerStatus = "ProducerStatus";
        public const string UpdateLatestSequenceDetail = "UpdateLatestSequenceDetail";
    }

    public class ServiceConstants
    {
        public const string MicroserviceHeaderKey = "MTN-SERVICENAME";
        public const string MicroserviceHeaderValue = "NavisSmart_OpsView";
        public const string MicroserviceUserName = "NavisSmart_OpsView";
    }
}