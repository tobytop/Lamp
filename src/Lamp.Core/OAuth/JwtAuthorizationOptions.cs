﻿using System;
using System.Collections.Generic;

namespace Lamp.Core.OAuth
{
    public class JwtAuthorizationOptions
    {
        public string ServerIp { get; set; }
        public int ServerPort { get; set; }
        public string SecretKey { get; set; }

        public bool ValidateLifetime { get; set; }
        public TimeSpan ExpireTimeSpan { get; set; }

        public bool ValidateIssuer { get; set; }
        public string ValidIssuer { get; set; }


        public bool ValidateAudience { get; set; }
        public string ValidAudience { get; set; }

        public string TokenEndpointPath { get; set; }

        public Action<JwtAuthorizationContext> CheckCredential;

        //public Dictionary<string, object> Payload { get; }
        public Dictionary<string, object> GetPayload()
        {
            Dictionary<string, object> payload = new Dictionary<string, object>();
            if (ValidateLifetime)
            {
                payload["exp"] = DateTime.Now.AddMinutes(ExpireTimeSpan.TotalMinutes).ToInt();
            }
            if (ValidateIssuer)
            {
                payload["iss"] = ValidIssuer;
            }
            if (ValidateAudience)
            {
                payload["aud"] = ValidAudience;
            }

            return payload;
        }

        //public string GetServiceId()
        //{
        //    return TokenEndpointPath.Replace("/", ".").Replace("\\", ".");
        //}
    }
}
