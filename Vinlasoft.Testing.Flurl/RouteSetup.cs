using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;

namespace AcmeSite.Tests.Faking
{
    public class RouteSetup
    {
        private readonly string _route;        
        private readonly Dictionary<HttpMethod, Expression> _methods;
        private readonly List<(string path, string request)> _requests;

        public RouteSetup(string route)
        {
            _route = route;            
            _requests = new List<(string, string)>();
            _methods = new Dictionary<HttpMethod, Expression>();
        }

        public string Route => _route;        

        public IEnumerable<(string path, string request)> Requests => _requests.ToList();

        public void AddMethod(HttpMethod method, Expression methodCall)
        {
            _methods[method] = methodCall;
        }

        public void LogRequest(string path, string request)
        {
            _requests.Add((path, request));
        }

        public RouteMatch MatchesOn(string requestPath)
        {
            var routeParts = _route.Split('/');
            var requestParts = requestPath.Split('/');
            var wildcards = new Dictionary<string, string>();

            if (routeParts.Length == requestParts.Length)
            {
                for (int i = 0; i < routeParts.Length; i++)
                {
                    var routePart = routeParts[i];
                    var requestPart = requestParts[i];

                    if (routePart.StartsWith("{"))
                    {
                        var wildcardKey = routePart.Substring(1, routePart.Length - 2);
                        wildcards.Add(wildcardKey, requestPart);
                    }
                    else if (routePart != requestPart)
                        return RouteMatch.NoMatch;
                }

                return new RouteMatch(this, wildcards);
            }

            return RouteMatch.NoMatch;
        }

        public string GetResponse(HttpMethod method, Dictionary<string, string> wildcards, string body)
        {
            if (!_methods.ContainsKey(method))
                throw new InvalidOperationException("Http method not set up for route");

            var expression = _methods[method];

            var resolver = new ExpressionResolver();
            return resolver.Resolve(expression, wildcards, body);
        }        
    }
}
