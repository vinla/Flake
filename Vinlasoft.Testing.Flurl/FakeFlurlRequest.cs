using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AcmeSite.Tests.Faking
{
    public class FakeFlurlRequest : IFlurlRequest
    {
        private IFlurlClient _client;
        private string _url;
        private RouteMatch _routeMatch;
        private Dictionary<string, object> _headers;
        private Dictionary<string, Cookie> _cookies;
        private FlurlHttpSettings _settings;

        public FakeFlurlRequest(IFlurlClient client, string route, RouteMatch routeMatch)
        {
            _url = client.BaseUrl + "/" + route;
            _routeMatch = routeMatch;
            _client = client;
            _headers = new Dictionary<string, object>();
            _cookies = new Dictionary<string, Cookie>();
            _settings = new FlurlHttpSettings();
            _settings.JsonSerializer = new DefaultJsonSerializer();
        }

        public IFlurlClient Client { get => _client; set => _client = value; }
        public Url Url { get => _url; set => _url = value; }
        public FlurlHttpSettings Settings { get => _settings; set => _settings = value; }

        public IDictionary<string, object> Headers => _headers;

        public IDictionary<string, Cookie> Cookies => _cookies;

        public async Task<HttpResponseMessage> SendAsync(HttpMethod verb, HttpContent content = null, CancellationToken? cancellationToken = null, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
        {
            var contentAsString = String.Empty;
            if (content != null)
                contentAsString = await content.ReadAsStringAsync();

            var response = new HttpResponseMessage
            {
                Content = new StringContent(_routeMatch.GetResponse(verb, contentAsString)),
                StatusCode = HttpStatusCode.Accepted,
                RequestMessage = new HttpRequestMessage()
            };

            response.RequestMessage.Properties.Add("FlurlHttpCall", new HttpCall
            {
                FlurlRequest = this
            });

            return response;
        }
    }

    public class DefaultJsonSerializer : ISerializer
    {
        public T Deserialize<T>(string s)
        {
            return JsonConvert.DeserializeObject<T>(s);
        }

        public T Deserialize<T>(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return Deserialize<T>(reader.ReadToEnd());
            }
        }

        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
