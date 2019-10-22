using Autofac;
using Autofac.Core;
using Lamp.Core.Common;
using Lamp.Core.Common.Logger;
using Lamp.Core.Common.TypeConverter;
using Lamp.Core.Filter;
using Lamp.Core.Protocol.Attributes;
using Lamp.Core.Protocol.Communication;
using Lamp.Core.Protocol.Server;
using Lamp.Core.Serializer;
using Lamp.Core.ServiceId;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Lamp.Core.Server.ServiceContainer
{
    public class ServiceEntryContainer : IServiceEntryContainer
    {
        //autofac容器
        private readonly IContainer _container;
        private readonly IServiceIdGenerator _serviceIdGenerate;
        private readonly ITypeConvertProvider _typeConvertProvider;

        private readonly ConcurrentDictionary<Tuple<Type, string>, FastExecutor.FastExecutorHandler> _handler;
        private readonly List<ServiceEntry> _services;
        private readonly ISerializer _serializer;
        private readonly ILogger _logger;

        public ServiceEntryContainer(IContainer container, IServiceIdGenerator serviceIdGenerate, ITypeConvertProvider typeConvertProvider, ISerializer serializer, ILogger logger)
        {
            _container = container;
            _services = new List<ServiceEntry>();
            _handler = new ConcurrentDictionary<Tuple<Type, string>, FastExecutor.FastExecutorHandler>();
            _serializer = serializer;
            _logger = logger;
            _serviceIdGenerate = serviceIdGenerate;
            _typeConvertProvider = typeConvertProvider;
        }

        public IServiceEntryContainer AddServices(Type[] types)
        {
            IEnumerable<Type> serviceTypes = types
                .Where(x => x.GetMethods().Any(y => y.GetCustomAttribute<ServiceAttribute>() != null)).Distinct();

            foreach (Type type in serviceTypes)
            {
                ServiceRouteAttribute routeTemplate = type.GetCustomAttribute<ServiceRouteAttribute>();
                ServiceVersionAttribute version = type.GetCustomAttribute<ServiceVersionAttribute>();

                foreach (MethodInfo methodInfo in type.GetTypeInfo().GetMethods().Where(x => x.GetCustomAttributes<ServiceDescAttribute>().Any()))
                {

                    ServiceDesc desc = new ServiceDesc();
                    IEnumerable<ServiceDescAttribute> descriptorAttributes = methodInfo.GetCustomAttributes<ServiceDescAttribute>();

                    IEnumerable<FilterBaseAttribute> filters = methodInfo.GetCustomAttributes<FilterBaseAttribute>();

                    foreach (ServiceDescAttribute attr in descriptorAttributes)
                    {
                        attr.Apply(desc);
                    }

                    desc.ReturnDesc = GetReturnDesc(methodInfo);

                    if (string.IsNullOrEmpty(desc.HttpMethod))
                    {
                        desc.HttpMethod = GetHttpMethod(methodInfo);
                    }

                    desc.Parameters = _serializer.Serialize<string>(GetParameters(methodInfo));

                    string route = string.Empty;

                    if (routeTemplate != null)
                    {
                        if (version != null && routeTemplate.RouteTemplate.Contains("{version}"))
                        {
                            route = routeTemplate.RouteTemplate.Replace("{version}", version.Version);
                            desc.Version = version.Version;
                        }
                        else
                        {
                            route = routeTemplate.RouteTemplate;
                        }
                    }
                    if (string.IsNullOrEmpty(desc.Id))
                    {
                        desc.Id = _serviceIdGenerate.GenerateServiceId(methodInfo, route, desc).ToLower();
                    }

                    FastExecutor.FastExecutorHandler fastInvoker = GetHandler(desc.Id, methodInfo);

                    ParameterInfo[] methodParas = methodInfo.GetParameters();
                    if (routeTemplate != null)
                    {
                        desc.RoutePath = ServiceRoute.ParseRoutePath(route, type.Name, methodInfo.Name, methodParas, type.IsInterface);
                    }

                    ServiceEntry service = new ServiceEntry
                    {
                        Descriptor = desc,
                        Parameters = methodParas,

                        Func = (context) =>
                        {
                            object instance = GetInstance(methodInfo.DeclaringType, context.RemoteInvokeMessage.Payload);

                            Dictionary<Type, object> dic = new Dictionary<Type, object>();
                            foreach (ParameterInfo p in methodParas)
                            {
                                context.RemoteInvokeMessage.Parameters.TryGetValue(p.Name.ToLower(), out object value);
                                object parameter;
                                Type paraType = p.ParameterType;
                                if (typeof(List<ServerFile>) == paraType)
                                {
                                    List<FileData> fileData = _typeConvertProvider.Convert(value, paraType) as List<FileData>;
                                    List<ServerFile> files = new List<ServerFile>();
                                    foreach (FileData file in fileData)
                                    {
                                        ServerFile serverFile = new ServerFile
                                        {
                                            Data = Convert.FromBase64String(file.Data),
                                            FileName = file.FileName
                                        };
                                        files.Add(serverFile);
                                    }
                                    parameter = files;
                                }
                                else
                                {
                                    parameter = _typeConvertProvider.Convert(value, paraType) ?? null;
                                }
                                dic[paraType] = parameter;
                            }

                            FilterContext filterContext = new FilterContext()
                            {
                                Payload = context.RemoteInvokeMessage.Payload,
                                Descriptor = desc,
                                ServiceArguments = dic
                            };

                            filters?.Aggregate(filterContext, (filtercontext, filter) =>
                            {
                                filter.Container = _container;
                                if (filtercontext.Result == null)
                                {
                                    filter.OnActionExecuting(filtercontext);
                                }
                                return filtercontext;
                            });

                            if (filterContext.Result == null)
                            {
                                filterContext.Result = fastInvoker(instance, dic.Select(o => o.Value).ToArray());

                                filters?.Aggregate(filterContext, (filtercontext, filter) =>
                                {
                                    filter.OnActionExecuted(filtercontext);
                                    return filtercontext;
                                });
                            }

                            return Task.FromResult(filterContext.Result);
                        }
                    };

                    _services.Add(service);
                }
            }

            return this;
        }

        private FastExecutor.FastExecutorHandler GetHandler(string key, MethodInfo method)
        {
            if (_handler.Count(o => o.Key.Item2 == key && o.Key.Item1 != method) > 0)
            {
                Exception ex = new Exception();
                _logger.Error("有重复路径:" + key, ex);
                throw ex;
            }
            _handler.TryGetValue(Tuple.Create(method.DeclaringType, key), out FastExecutor.FastExecutorHandler handler);
            if (handler == null)
            {
                handler = FastExecutor.GetMethodInvoker(method);
                _handler.GetOrAdd(Tuple.Create(method.DeclaringType, key), handler);
            }

            return handler;
        }

        private object GetInstance(Type type, Payload header)
        {
            // all service are instancePerDependency, to avoid resolve the same isntance , so we add using scop here
            using (ILifetimeScope scope = _container.BeginLifetimeScope())
            {
                return scope.Resolve(type, new ResolvedParameter(
                            (pi, ctx) => pi.ParameterType == typeof(Payload),
                            (pi, ctx) => header
                        ));
            }
        }

        private List<ServiceParameterDesc> GetParameters(MethodInfo method)
        {
            //StringBuilder sb = new StringBuilder();
            List<ServiceParameterDesc> paras = new List<ServiceParameterDesc>();
            IEnumerable<FieldCommentAttribute> paraComments = method.GetCustomAttributes<FieldCommentAttribute>();
            ParameterInfo[] methodparas = method.GetParameters();

            //if (methodparas.Count(x => !x.ParameterType.FullName.StartsWith("System.")) > 0 && methodparas.Count() > 1)
            //{
            //    string message = "如果方法的参数为自定义类型，则参数只能有一个";
            //    Exception e = new Exception(message);
            //    _logger.Error(message, e);
            //    throw e;
            //}

            foreach (ParameterInfo para in methodparas)
            {
                ServiceParameterDesc paraDesc = new ServiceParameterDesc
                {
                    Name = para.Name.ToLower(),
                    Type = para.ParameterType.ToString(),
                    Comment = paraComments.FirstOrDefault(x => x.FieldName == para.Name)?.Comment,
                };
                if (para.ParameterType.IsClass
                && !para.ParameterType.FullName.StartsWith("System."))
                {
                    //var t = Activator.CreateInstance(para.ParameterType);
                    //sb.Append($"\"{para.Name}\":{_serializer.Serialize<string>(t)},");
                    //sb.Append($"\"{para.Name}\":{{{GetCustomTypeMembers(para.ParameterType)}}},");
                    paraDesc.Format = $"{{{ GetCustomTypeMembers(para.ParameterType)}}}";
                }
                else
                {
                    paraDesc.Format = $"{para.ParameterType.ToString()}";
                    //sb.Append($"\"{para.Name}\":\"{para.ParameterType.ToString()}\",");
                }
                paras.Add(paraDesc);
            }
            //return "{" + sb.ToString().TrimEnd(',') + "}";
            return paras;
        }

        private string GetReturnDesc(MethodInfo methodInfo)
        {
            ServiceReturnDesc desc = new ServiceReturnDesc();


            List<Type> customTypes = new List<Type>();
            //判断是否为task泛型
            if (methodInfo.ReturnType.ToString().IndexOf("System.Threading.Tasks.Task", StringComparison.Ordinal) == 0 &&
                      methodInfo.ReturnType.IsGenericType)
            {
                desc.ReturnType = string.Join(",", methodInfo.ReturnType.GenericTypeArguments.Select(x => x.FullName));
                customTypes = (from type in methodInfo.ReturnType.GenericTypeArguments
                               from childType in type.GenericTypeArguments
                               select childType).ToList();
            }
            else if (methodInfo.ReturnType.IsGenericType)
            {
                desc.ReturnType = methodInfo.ReturnType.ToString();
                customTypes = methodInfo.ReturnType.GenericTypeArguments.ToList();
            }
            else
            {
                desc.ReturnType = methodInfo.ReturnType.ToString();
                customTypes = new List<Type> { methodInfo.ReturnType };
            }

            if (string.IsNullOrEmpty(desc.ReturnFormat))
            {
                desc.ReturnFormat = GetReturnFormat(customTypes);
            }

            return _serializer.Serialize<string>(desc);
        }

        private string GetReturnFormat(List<Type> types)
        {
            StringBuilder sb = new StringBuilder();

            foreach (Type customType in types)
            {
                if (customType.IsClass
                 && !customType.FullName.StartsWith("System."))
                {
                    sb.Append($"{{{ GetCustomTypeMembers(customType)}}},");
                }
                //else if (customType.FullName.StartsWith("System.Collections.Generic"))
                //{
                //    var childTypes = customType.GenericTypeArguments.ToList();
                //    sb.Append($"[{GetReturnFormat(childTypes)}]");
                //}
                else
                {
                    sb.Append($"{customType.ToString()},");
                }
            }
            return sb.ToString().TrimEnd(',');
        }

        private string GetCustomTypeMembers(Type customType)
        {
            StringBuilder sb = new StringBuilder(); ;
            foreach (PropertyInfo prop in customType.GetProperties())
            {
                if (prop.PropertyType.IsClass && !prop.PropertyType.FullName.StartsWith("System."))
                {

                    sb.Append($"\"{prop.Name}\":{{{GetCustomTypeMembers(prop.PropertyType).TrimEnd(',')}}}");
                }
                else
                {
                    FieldCommentAttribute comment = prop.GetCustomAttribute<FieldCommentAttribute>();
                    string proComment = comment == null ? "" : (" | " + comment?.Comment);
                    sb.Append($"\"{prop.Name}\":\"{prop.PropertyType.ToString()}{proComment}\",");
                }
            }
            return sb.ToString();
        }
        public List<ServiceEntry> GetServiceEntry()
        {
            return _services;
        }

        private string GetHttpMethod(MethodInfo method)
        {
            return method.GetParameters().Any(x => x.ParameterType.IsClass
                && !x.ParameterType.FullName.StartsWith("System.")) ? "POST" : "GET";
        }
    }
}
