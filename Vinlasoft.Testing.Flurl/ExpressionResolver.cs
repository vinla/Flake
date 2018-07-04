using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;

namespace AcmeSite.Tests.Faking
{
     public class ExpressionResolver
    {
        public string Resolve<TOut>(Expression<Func<TOut>> func, params (string, object)[] args)
        {
            return Resolve(func.Body, args.ToDictionary(x => x.Item1, x => x.Item2.ToString()), String.Empty);
        }

        public string Resolve<TIn, TOut>(Expression<Func<TIn, TOut>> func, params (string, object)[] args)
        {
            return Resolve(func.Body, args.ToDictionary(x => x.Item1, x => x.Item2.ToString()), String.Empty);
        }

        public string Resolve(Expression expression, Dictionary<string, string> wildcards, string body)
        {
            if (expression is MethodCallExpression methodCallExpression)
                return GetResponse(methodCallExpression, wildcards, body);

            if (expression is MemberInitExpression memberInit)
                return GetResponse(memberInit, wildcards);

            if (expression is InvocationExpression invocationExpression)
            {
                var mappedArguments = new List<Expression>();
                foreach (var argument in invocationExpression.Arguments)
                {
                    if (argument is ConstantExpression)
                        mappedArguments.Add(argument);
                    else if (argument is ParameterExpression param)
                        mappedArguments.Add(GetExpression(param, wildcards));
                }
                var mappedExpression = invocationExpression.Update(invocationExpression.Expression, mappedArguments);
                return Invoke(mappedExpression);
            }

            if(expression is BinaryExpression binaryExpression)
            {
                var mappedLeft = binaryExpression.Left is ParameterExpression pexLeft ? GetExpression(pexLeft, wildcards) : binaryExpression.Left;
                var mappedRight = binaryExpression.Right is ParameterExpression pexRight ? GetExpression(pexRight, wildcards) : binaryExpression.Right;

                var mappedExpression = binaryExpression.Update(mappedLeft, binaryExpression.Conversion, mappedRight);
                return Invoke(mappedExpression);
            }
            
            throw new NotSupportedException("Expression type is not supported");
        }

        private string GetResponse(MemberInitExpression expression, Dictionary<string, string> wildcards)
        {
            var mappedBindgings = new List<MemberAssignment>();

            var bindings = expression.Bindings.OfType<MemberAssignment>();
            foreach (var binding in bindings)
            {
                if (binding.Expression is ConstantExpression)
                    mappedBindgings.Add(binding);
                else if (binding.Expression is ParameterExpression param)
                    mappedBindgings.Add(binding.Update(GetExpression(param, wildcards)));                
            }

            var mappedExpression = expression.Update(expression.NewExpression, mappedBindgings);
            return Invoke(mappedExpression);
        }

        private string GetResponse(MethodCallExpression methodCallExpression, Dictionary<string, string> wildcards, string body)
        {
            object target = null;
            MemberExpression memberExpression = (MemberExpression)methodCallExpression.Object;

            if (memberExpression != null)
            {
                Expression<Func<Object>> getCallerExpression = Expression<Func<Object>>.Lambda<Func<Object>>(memberExpression);
                Func<Object> getCaller = getCallerExpression.Compile();
                target = getCaller();
            }

            var arguments = new List<object>();
            var parameters = methodCallExpression.Method.GetParameters();

            foreach (var parameter in parameters)
            {
                if (wildcards.ContainsKey(parameter.Name))
                {
                    var temp = wildcards[parameter.Name];
                    arguments.Add(JsonConvert.DeserializeObject(temp, parameter.ParameterType));
                }
                else if (parameter.CustomAttributes.Any())
                {
                    arguments.Add(JsonConvert.DeserializeObject(body, parameter.ParameterType));
                }
                else
                    arguments.Add(null);
            }

            var result = methodCallExpression.Method.Invoke(target, arguments.ToArray());
            return JsonConvert.SerializeObject(result);
        }

        private ConstantExpression GetExpression(ParameterExpression param, Dictionary<string, string> wildcards)
        {
            var temp = wildcards[param.Name];
            return Expression.Constant(JsonConvert.DeserializeObject(temp, param.Type));
        }

        private string Invoke(Expression expression)
        {
            var lambda = Expression.Lambda(expression);
            var compiled = lambda.Compile();
            return JsonConvert.SerializeObject(compiled.DynamicInvoke());
        }
    }
}