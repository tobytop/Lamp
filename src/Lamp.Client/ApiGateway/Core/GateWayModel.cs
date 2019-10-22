using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Lamp.Client.ApiGateway.Core
{
    public class GateWayModel
    {
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
        public GateWayModel() { }
        public GateWayModel(Stream content)
        {
            using (StreamReader sr = new StreamReader(content))
            {
                string json = sr.ReadToEnd();
                Data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            }

        }

        public GateWayModel(IFormCollection form)
        {
            foreach (KeyValuePair<string, StringValues> f in form)
            {
                Data.Add(f.Key, f.Value);
            }
        }
        public GateWayModel(Dictionary<string, object> data)
        {
            Data = data;
        }
    }
}
