using Lamp.Core.Protocol.Communication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Lamp.Client.ApiGateway.Core
{
    public class GateWayModelBinder : IModelBinder
    {
        private readonly IModelBinder _modelBinder;
        public GateWayModelBinder(IModelBinder modelBinder)
        {
            _modelBinder = modelBinder;
        }
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            GateWayModel model = null;
            HttpRequest req = bindingContext.ActionContext.HttpContext.Request;
            if (req.HasFormContentType)
            {
                IFormCollection form = req.Form;
                if (form != null && (form.Any() || form.Files.Any()))
                {
                    if (form.Files.Any())
                    {
                        List<FileData> list = new List<FileData>();
                        foreach (IFormFile file in form.Files)
                        {
                            using (Stream sr = file.OpenReadStream())
                            {
                                byte[] bytes = new byte[sr.Length];
                                sr.ReadAsync(bytes, 0, bytes.Length);
                                FileData thisFile = new FileData
                                {
                                    FileName = file.FileName,
                                    Data = Convert.ToBase64String(bytes)
                                };
                                list.Add(thisFile);
                            }
                        }

                        Dictionary<string, object> data = new Dictionary<string, object> { { "files", list } };
                        model = new GateWayModel(data);
                    }
                    else
                    {
                        model = new GateWayModel(form);
                    }
                }
            }
            else
            {
                Stream body = req.Body;
                if (body != null)
                {
                    try
                    {
                        model = new GateWayModel(body);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            bindingContext.ModelState.SetModelValue("model", model, null);
            bindingContext.Result = ModelBindingResult.Success(model);
            return Task.CompletedTask;
        }
    }
}
