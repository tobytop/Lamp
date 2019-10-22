using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Lamp.Core.Protocol.Communication;
using Lamp.Core.Serializer;
using Lamp.Core.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lamp.Core.OAuth
{
    public class JwtAuthorizationMiddleware
    {
        private readonly RequestDel _next;
        private readonly JwtAuthorizationOptions _options;
        private readonly ISerializer _serializer;
        public JwtAuthorizationMiddleware(RequestDel next, JwtAuthorizationOptions options, ISerializer serializer)
        {
            _options = options;
            _serializer = serializer;
            _next = next;
        }

        public Task Invoke(RemoteExecutorContext context)
        {
            // get jwt token 
            if (!string.IsNullOrEmpty(_options.TokenEndpointPath)
                && context.ServiceEntry == null
                && context.RemoteInvokeMessage.ServiceId == _options.TokenEndpointPath.TrimStart('/'))
            {
                if (_options.CheckCredential == null)
                {
                    throw new Exception("JwtAuthorizationOptions.CheckCredential must be provided");
                }

                JwtAuthorizationContext jwtAuthorizationContext = new JwtAuthorizationContext(_options, context.RemoteInvokeMessage);

                _options.CheckCredential(jwtAuthorizationContext);
                if (jwtAuthorizationContext.IsRejected)
                {
                    return context.Response.WriteAsync(context.TransportMessage.Id, new RemoteCallBackData
                    {
                        ErrorMsg = $"{jwtAuthorizationContext.Error}, {jwtAuthorizationContext.ErrorDescription}",
                        ErrorCode = "400"
                    });
                }

                Dictionary<string, object> payload = jwtAuthorizationContext.GetPayload();
                IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
                IJsonSerializer serializer = new JsonNetSerializer();
                IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);
                string token = encoder.Encode(payload, _options.SecretKey);

                IDictionary<string, object> result = new Dictionary<string, object>
                {
                    ["access_token"] = token
                };
                if (_options.ValidateLifetime)
                {
                    result["expired_in"] = payload["exp"];
                }

                return context.Response.WriteAsync(context.TransportMessage.Id, new RemoteCallBackData
                {
                    Result = result
                });
            }
            else if (context.ServiceEntry != null && context.ServiceEntry.Descriptor.EnableAuthorization)
            {

                try
                {
                    string pureToken = context.RemoteInvokeMessage.Token;
                    if (pureToken != null && pureToken.Trim().StartsWith("Bearer "))
                    {
                        pureToken = pureToken.Trim().Substring(6).Trim();
                    }
                    else
                    {
                        return context.Response.WriteAsync(context.TransportMessage.Id, Unauthorized("Unauthorized"));
                    }

                    IJsonSerializer serializer = new JsonNetSerializer();
                    IDateTimeProvider provider = new UtcDateTimeProvider();
                    IJwtValidator validator = new JwtValidator(serializer, provider);
                    IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                    IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder);
                    string payload = decoder.Decode(pureToken, _options.SecretKey, verify: true);

                    IDictionary<string, object> payloadObj = _serializer.Deserialize<string, Dictionary<string, object>>(payload);
                    if (_options.ValidateLifetime)
                    {
                        //var exp = payloadObj["exp"];
                        if (payloadObj == null || (long.Parse(payloadObj["exp"].ToString()).ToDate() < DateTime.Now))
                        {
                            return context.Response.WriteAsync(context.TransportMessage.Id, Unauthorized("Token is Expired"));
                        }
                    }
                    string serviceRoles = context.ServiceEntry.Descriptor.Roles;
                    if (!string.IsNullOrEmpty(serviceRoles))
                    {
                        string[] serviceRoleArr = serviceRoles.Split(',');
                        string roles = payloadObj != null && payloadObj.ContainsKey("roles") ? payloadObj["roles"] + "" : "";
                        bool authorize = roles.Split(',').Any(role => serviceRoleArr.Any(x => x.Equals(role, StringComparison.InvariantCultureIgnoreCase)));
                        if (!authorize)
                        {
                            return context.Response.WriteAsync(context.TransportMessage.Id, Unauthorized("Unauthorized"));
                        }
                    }
                    context.RemoteInvokeMessage.Payload = new Payload { Items = payloadObj };
                }
                catch (Exception ex)
                {
                    return context.Response.WriteAsync(context.TransportMessage.Id, Unauthorized($"Token is incorrect, exception is { ex.Message}"));
                }
                return _next(context);
            }
            // service can be annoymouse request

            return _next(context);
        }
        private RemoteCallBackData Unauthorized(string message)
        {
            return new RemoteCallBackData
            {
                ErrorMsg = message,
                ErrorCode = "401"
            };
        }
    }
}
