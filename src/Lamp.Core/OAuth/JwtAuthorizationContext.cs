﻿using Lamp.Core.Protocol.Communication;
using System.Collections.Generic;

namespace Lamp.Core.OAuth
{
    public class JwtAuthorizationContext
    {
        public string Error { get; private set; }
        public string ErrorDescription { get; private set; }
        public bool IsRejected { get; private set; }

        public string UserName { get; }
        public string Password { get; }

        public RemoteCallData RemoteInvokeMessage { get; }


        private Dictionary<string, object> Payload { get; }

        public JwtAuthorizationContext(JwtAuthorizationOptions options, RemoteCallData remoteInvokeMessage)
        {
            Options = options;
            Payload = options.GetPayload();
            RemoteInvokeMessage = remoteInvokeMessage;
            if (remoteInvokeMessage.Parameters.ContainsKey("username"))
            {
                UserName = remoteInvokeMessage.Parameters["username"] + "";
            }

            if (remoteInvokeMessage.Parameters.ContainsKey("password"))
            {
                Password = remoteInvokeMessage.Parameters["password"] + "";
            }

            if (!string.IsNullOrEmpty(UserName))
            {
                Payload.Add("username", UserName);
            }
        }
        //public Dictionary<string, string> Claims { get; }

        public JwtAuthorizationOptions Options { get; }
        public void Rejected(string error, string errorDescription)
        {
            Error = error;
            ErrorDescription = errorDescription;
            IsRejected = true;
        }

        public void AddClaim(string name, string value)
        {
            Payload[name] = value;
        }

        public Dictionary<string, object> GetPayload()
        {
            return Payload;
        }
    }
}
