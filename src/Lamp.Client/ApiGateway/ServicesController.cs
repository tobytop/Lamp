using Autofac;
using Lamp.Client.ApiGateway.Core;
using Lamp.Core.Protocol.Communication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lamp.Client.ApiGateway
{
    public class ServicesController : Controller
    {
        private readonly IContainer _container;
        public ServicesController(IContainer container)
        {
            _container = container;
        }

        [HttpGet, HttpPost]
        public async Task<IActionResult> ExecutePath(string path, [FromQuery]GateWayQueryString query, [ModelBinder]GateWayModel model)
        {
            Dictionary<string, object> paras = new Dictionary<string, object>();
            if (model?.Data != null)
            {
                paras = model.Data;
            }
            if (query.Collection.Count > 0)
            {
                foreach (string key in query.Collection.AllKeys)
                {
                    paras[key.ToLower()] = query.Collection[key];
                }
            }
            RemoteCallBackData result = await GateWayClient.Invoke(path, paras, Request.Method, _container);

            if (result.ResultType != typeof(FileData).ToString())
            {
                return new JsonResult(result.Result);
            }

            FileData file = result.Result as FileData;
            return File(file?.Data, "application/octet-stream", file?.FileName);
        }
    }
}
