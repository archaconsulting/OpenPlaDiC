using Azure.Core;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Mvc;
using OpenPlaDiC.BIZ;
using OpenPlaDiC.Framework;
using System.Net;
using OpenPlaDiC.WebApp.Extensions;

namespace OpenPlaDiC.WebApp.Controllers
{
    public class APIController : Controller
    {
        private readonly static string _conventionFolderPath = @"C:\Proyectos\OpenPlaDiC\OpenPlaDiC.WebApp\Views";
        //private readonly IRazorRenderService _renderer;
        private readonly IDataService _dataService;

        public APIController(IDataService dataService)
        {
            _dataService = dataService;
            //_renderer = renderer;

        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Important for security
        public IActionResult GetProcData([FromBody] ProcDataModel model)
        {


            try
            {

                if (model.ProcName == null)
                {
                    return Json(new Response { Message = "Procedure name is required." });
                }
                else if (model.Parameters == null)
                {
                    var resp = _dataService.ExecProc(model.ProcName);


                    return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(resp));


                    //return Ok(resp);

                }
                else
                {
                    var resp = _dataService.ExecProc(model.ProcName, model.Parameters.ToArray());
                    return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(resp));
                }

            }
            catch (Exception ex)
            {
                return Json(new Response { Message = ex.Message, InnerException = ex.InnerException != null ? ex.InnerException.Message : "" });
            }



        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Important for security
        public async Task<IActionResult> GetSqlDataAsync([FromBody] QueryDataModel model)
        {


            try
            {

                if (model.SQLQuery == null)
                {
                    return Json(new Response { Message = "Query is required." });
                }
                else if (model.Parameters == null)
                {
                    var resp = await _dataService.GetQueryAsync(model.SQLQuery);
                    return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(resp));

                    //return Ok(resp);

                }
                else
                {
                    var resp = _dataService.GetQueryAsync(model.SQLQuery, model.Parameters.ToArray());
                    return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(resp));
                }

            }
            catch (Exception ex)
            {
                return Json(new Response { Message = ex.Message, InnerException = ex.InnerException != null ? ex.InnerException.Message : "" });
            }



        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Important for security
        public async Task<IActionResult> ExecSqlAsync([FromBody] QueryDataModel model)
        {


            try
            {

                if (model.SQLQuery == null)
                {
                    return Json(new Response { Message = "Query is required." });
                }
                else if (model.Parameters == null)
                {
                    var resp = await _dataService.ExecQueryAsync(model.SQLQuery);
                    return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(resp));

                    //return Ok(resp);

                }
                else
                {
                    var resp = _dataService.GetQueryAsync(model.SQLQuery, model.Parameters.ToArray());
                    return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(resp));
                }

            }
            catch (Exception ex)
            {
                return Json(new Response { Message = ex.Message, InnerException = ex.InnerException != null ? ex.InnerException.Message : "" });
            }



        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetQueryAsync([FromBody] QueryDataModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.SQLQuery))
                    return Json(new Response { Message = "Query is required." });

                // Invocación a IDataService.GetQueryAsync
                var resp = await _dataService.GetQueryAsync(model.SQLQuery, model.Parameters?.ToArray() ?? Array.Empty<GlobalItem>());
                return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(resp));
            }
            catch (Exception ex)
            {
                return Json(new Response { Message = ex.Message });
            }
        }

        public class ProcDataModel 
        {
            public string? ProcName { get; set; }
            public List<GlobalItem>? Parameters { get; set; }
        }

        public class QueryDataModel
        {
            public string? SQLQuery { get; set; }
            public List<GlobalItem>? Parameters { get; set; }
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ExecAPI([FromBody] Framework.Request request)
        //{
        //    try
        //    {
        //        string controller = "Custom";
        //        string action = request.View;
        //        var actionController = $"{controller}/{action}";


        //        string viewPath = System.IO.Path.Combine(_conventionFolderPath, actionController);


        //        var html = await _renderer.RenderToStringAsync(actionController, request);

        //        //engine.RunCompile(actionController, request.GetType(), request);

        //        html = html.Replace("\n", "");
        //        html = html.Replace("\r", "");
        //        //html = Framework.Helper.Base64Decode(html);


        //        return Ok(html);
        //    }
        //    catch (Exception ex)
        //    {
        //        //return BadRequest(ex.Message);
        //        return Ok(new Response { IsException = true, Message = ex.Message });

        //    }
        //}


    }
}




