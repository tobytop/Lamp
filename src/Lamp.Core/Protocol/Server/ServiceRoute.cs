using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Lamp.Core.Protocol.Server
{
    public class ServiceRoute
    {
        public List<ServerAddress> Address { get; set; }

        public ServiceDesc ServiceDescriptor { get; set; }

        public static string ParseRoutePath(string routeTemplete, string service, string method,
            ParameterInfo[] parameterInfos, bool isInterface)
        {
            StringBuilder result = new StringBuilder();
            string[] parameters = routeTemplete?.Split('/');
            foreach (string parameter in parameters)
            {
                string param = GetParameters(parameter).FirstOrDefault();
                if (param == null)
                {
                    result.Append($"{parameter}/");
                }
                else if (service.EndsWith(param))
                {
                    string curService = isInterface ? service.TrimStart('I') : service;
                    curService = curService.Substring(0, curService.Length - param.Length);
                    result.Append($"{curService}/");
                }
                //else if (param == "Method")
                //{
                //    result.Append(method);
                //}
                //result.Append("/");
            }

            result.Append(method);
            result = new StringBuilder(result.ToString().ToLower());

            if (!parameterInfos.Any())
            {
                return result.ToString().TrimEnd('&', '/', '\\').TrimStart('/', '\\');
            }

            result.Append("?");
            foreach (ParameterInfo para in parameterInfos)
            {
                //if (para.IsOptional)
                //{
                //    result.Append($"[{para.Name}]=&");
                //}
                //else
                //{
                result.Append($"{para.Name}=&");
            }
            //}

            return result.ToString().TrimEnd('&', '/', '\\').TrimStart('/', '\\');
        }

        private static List<string> GetParameters(string text)
        {
            List<string> matchVale = new List<string>();
            string reg = @"(?<={)[^{}]*(?=})";
            string key = string.Empty;
            foreach (Match m in Regex.Matches(text, reg))
            {
                matchVale.Add(m.Value);
            }

            return matchVale;
        }
    }
}
