using Lamp.Core.Client.Discovery;
using Lamp.Core.Protocol.Server;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Lamp.Swagger
{
    public class RpcSwaggerDocumentFilter : IDocumentFilter
    {
        private IClientServiceDiscovery _clientServiceDiscovery;
        public RpcSwaggerDocumentFilter(IClientServiceDiscovery clientServiceDiscovery)
        {
            _clientServiceDiscovery = clientServiceDiscovery;
        }

        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            List<ServerDesc> servers = _clientServiceDiscovery.GetRoutesAsync().GetAwaiter().GetResult();
            Dictionary<string, ServiceDesc> routes = new Dictionary<string, ServiceDesc>();

            foreach (ServerDesc descriptor in servers)
            {
                descriptor.ServiceDescriptor.ForEach(x =>
                {
                    if (!routes.ContainsKey(x.Id) && (swaggerDoc.Info.Version == "all" || x.Version == swaggerDoc.Info.Version))
                    {
                        routes[x.Id] = (ServiceDesc)x.Clone();
                    }
                });
            }

            foreach (KeyValuePair<string, ServiceDesc> r in routes)
            {
                List<IParameter> paras = new List<IParameter>();
                if (!string.IsNullOrEmpty(r.Value.Parameters))
                {
                    List<ServiceParameterDesc> parameters = JsonConvert.DeserializeObject(TypeHelper.ReplaceTypeToJsType(r.Value.Parameters), typeof(List<ServiceParameterDesc>)) as List<ServiceParameterDesc>;
                    paras = GetParameters(parameters, r.Value.HttpMethod);
                }

                if (r.Value.GetMetadata<bool>("EnableAuthorization"))
                {
                    paras.Add(new NonBodyParameter
                    {
                        Name = "Authorization",
                        Type = "string",
                        In = "header",
                        Description = "Token",
                        Required = true,
                        Default = "Bearer "
                    });
                }

                Dictionary<string, Response> response = new Dictionary<string, Response>
                {
                    { "200", GetResponse(r.Value.ReturnDesc) }
                };

                string tag = r.Key.Substring(0, r.Key.IndexOf("/"));
                tag = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tag);

                if (r.Value.HttpMethod == "GET")
                {
                    swaggerDoc.Paths.Add("/" + r.Key, new PathItem
                    {
                        Get = new Operation
                        {
                            Consumes = new List<string> { "application/json" },
                            OperationId = r.Key,
                            Parameters = paras,
                            Produces = new List<string> { "application/json" },
                            Responses = response,
                            Summary = r.Value.Comment ?? "",
                            Tags = new List<string> { tag }
                        }
                    });
                }
                else
                {
                    swaggerDoc.Paths.Add("/" + r.Key, new PathItem
                    {
                        Post = new Operation
                        {
                            Consumes = new List<string> { "application/json" },
                            OperationId = r.Key,
                            Parameters = paras,
                            Produces = new List<string> { "application/json" },
                            Responses = response,
                            Summary = r.Value.Comment ?? "",
                            Tags = new List<string> { tag }
                        }
                    });
                }
            }
        }

        private static Response GetResponse(string returnDescStr)
        {

            if (string.IsNullOrEmpty(returnDescStr) || !returnDescStr.StartsWith("{"))
            {
                return new Response
                {
                    Description = "Success",
                    Schema = new Schema
                    {
                        Type = returnDescStr
                    }
                };
            }
            ServiceReturnDesc returnDesc = JsonConvert.DeserializeObject<ServiceReturnDesc>(TypeHelper.ReplaceTypeToJsType(returnDescStr));
            bool isObject = TypeHelper.CheckIsObject(returnDesc.ReturnType);
            Response response = new Response
            {
                Description = string.IsNullOrEmpty(returnDesc.Comment) ? "Success" : returnDesc.Comment,
                Schema = new Schema
                {
                    Type = isObject ? "object" : returnDesc.ReturnType,
                    Example = (isObject && returnDesc.ReturnFormat.StartsWith("{")) ? JsonConvert.DeserializeObject<dynamic>(returnDesc.ReturnFormat) : returnDesc.ReturnFormat,
                }
            };
            bool isArray = TypeHelper.CheckIsArray(returnDesc.ReturnType);
            if (isArray)
            {
                response.Schema.Example = (isObject && returnDesc.ReturnFormat.StartsWith("{")) ? JsonConvert.DeserializeObject<dynamic>($"[{returnDesc.ReturnFormat}]") : $"[{returnDesc.ReturnFormat}]";
            }
            return response;
        }

        private static List<IParameter> GetParameters(List<ServiceParameterDesc> paras, string httpMethod)
        {
            List<IParameter> parameters = new List<IParameter>();
            int idx = 0;
            StringBuilder sbExample = new StringBuilder();
            foreach (ServiceParameterDesc p in paras)
            {
                idx++;
                if (httpMethod == "GET")
                {
                    NonBodyParameter param = new NonBodyParameter
                    {
                        Name = p.Name,
                        Type = p.Type,
                        //Format = typeInfo.Format,
                        In = "query",
                        Description = $"{p.Comment}",
                    };
                    //if (typeInfo.IsArray)
                    if (TypeHelper.CheckIsArray(p.Type))
                    {
                        param.Format = null;
                        param.Items = new PartialSchema
                        {
                            //Type = typeInfo.Type
                            Type = TypeHelper.GetArrayType(p.Type)
                        };
                        param.Type = "array";
                    }
                    parameters.Add(param);
                }
                else
                {

                    BodyParameter bodyPara = new BodyParameter
                    {
                        Name = p.Name,
                        In = "body",
                        Description = $"{p.Comment}",
                        Schema = new Schema
                        {
                            Format = p.Format,
                        }

                    };
                    // swagger bug: two or more object parameter in post, when execute it, just post the last one,so we put all parameter in the last one that it can post it
                    if (!string.IsNullOrEmpty(p.Format) && p.Format.IndexOf("{") < 0)
                    {
                        sbExample.Append($"{p.Name}:\"{ p.Format}\",");
                    }
                    else if (!string.IsNullOrEmpty(p.Format))
                    {

                        sbExample.Append($"{p.Name}:{ p.Format},");
                    }
                    if (idx == paras.Count && sbExample.Length > 0 && paras.Count > 1)
                    {
                        bodyPara.Schema.Example = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>($"{{{sbExample.ToString().TrimEnd(',')}}}");
                    }
                    else if (idx == paras.Count && sbExample.Length > 0)
                    {
                        bodyPara.Schema.Example = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>($"{{{sbExample.ToString().TrimEnd(',')}}}");

                    }

                    parameters.Add(bodyPara);
                }
            }
            return parameters;
        }
    }
}
