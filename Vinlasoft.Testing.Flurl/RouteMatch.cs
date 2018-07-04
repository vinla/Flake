using System.Collections.Generic;
using System.Net.Http;

namespace AcmeSite.Tests.Faking
{
    public class RouteMatch
    {
        private RouteSetup _routeSetup;
        private readonly bool _success;
        private readonly Dictionary<string, string> _wildcards;

        private RouteMatch(bool success)
        {
            _success = success;
        }

        public RouteMatch(RouteSetup routeSetup, Dictionary<string, string> wildcards)
        {
            _success = true;
            _wildcards = wildcards;
            _routeSetup = routeSetup;
        }

        public static RouteMatch NoMatch => new RouteMatch(false);

        public bool Success => _success;

        public RouteSetup Setup => _routeSetup;

        public string GetResponse(HttpMethod method, string body)
        {
            return _routeSetup.GetResponse(method, _wildcards, body);
        }               
    }
}
