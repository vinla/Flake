using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AcmeSite.Tests.Faking
{
    public class FakeFlurlClient : IFlurlClient
    {
        private List<RouteSetup> _routes;
        private Dictionary<string, object> _headers;
        private Dictionary<string, Cookie> _cookies;
        private ClientFlurlHttpSettings _settings;
        private string _baseUrl;

        public FakeFlurlClient(string baseUrl)
        {
            _baseUrl = baseUrl;
            _headers = new Dictionary<string, object>();
            _cookies = new Dictionary<string, Cookie>();
            _settings = new ClientFlurlHttpSettings();
            _routes = new List<RouteSetup>();
        }
        
        public ClientFlurlHttpSettings Settings { get => _settings; set => _settings = value; }

        public HttpClient HttpClient => throw new NotSupportedException("This is a fake client");

        public HttpMessageHandler HttpMessageHandler => throw new NotSupportedException("This is a fake client");

        public string BaseUrl { get => _baseUrl; set => _baseUrl = value; }

        public bool IsDisposed => false;

        public IDictionary<string, object> Headers => _headers;

        public IDictionary<string, Cookie> Cookies => _cookies;

        FlurlHttpSettings IHttpSettingsContainer.Settings { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool CheckAndRenewConnectionLease() => true;
        
        public void Dispose()
        {
            
        }

        public void SetupRouteGet(string route, object data)
        {            
            var response = JsonConvert.SerializeObject(data);
            Func<object> r = () => { return JsonConvert.DeserializeObject(response); };
            SetupRoute(route, HttpMethod.Get, () => r.Invoke());
        }

        public void SetupRoute(string route, HttpMethod method, Expression<Action> func)
        {
            SetupRoute(route, method, func.Body);
        }

        public void SetupRoute<TIn>(string route, HttpMethod method, Expression<Action<TIn>> func)
        {
            SetupRoute(route, method, func.Body);
        }

        public void SetupRoute<TIn, TOut>(string route, HttpMethod method, Expression<Func<TIn, TOut>> func)
        {
            SetupRoute(route, method, func.Body);
        }

        private void SetupRoute(string route, HttpMethod method, Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            var routeSetup = _routes.SingleOrDefault(r => r.Route == route);
            if(routeSetup == null)
            {
                routeSetup = new RouteSetup(route);
                _routes.Add(routeSetup);
            }

            routeSetup.AddMethod(method, expression);
        }

        public IFlurlRequest Request(params object[] urlSegments)
        {
            var route = String.Join("/", urlSegments);
            var routeMatch = _routes.Select(r => r.MatchesOn(route)).FirstOrDefault(rm => rm.Success);

            if (routeMatch == null)
                throw new InvalidOperationException("Route has not been set up");

            return new FakeFlurlRequest(this, route, routeMatch);
        }
    }       
}
