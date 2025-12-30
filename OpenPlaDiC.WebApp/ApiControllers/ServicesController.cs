using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Tasks;
using OpenPlaDiC.BIZ.Services;
using OpenPlaDiC.DAL.Repositories;
using OpenPlaDiC.Framework;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace OpenPlaDiC.WebApp.ApiControllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {

        private readonly static string _conventionFolderPath = @"C:\Proyectos\OpenPlaDiC\OpenPlaDiC.WebApp\Views";
        private readonly IRazorRenderService _renderer;
        private readonly IDataService _daService;

        //System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Custom");

        //private IRazorEngineService _razorEngineService;

        public ServicesController(IRazorRenderService renderer, IDataService daService)
        {
            _renderer = renderer;
            _daService = daService;
        }

        [HttpPost]
        public ActionResult ExecAPI2(Request request)
        {

            try
            {

                string controller = "Custom";
                string action = request.View;
                string usingHeader = "@using OpenPlaDiC.WebApp.Extensions ";
                string getResult = " @Html.CreateApiResponse(response)";

                var templateConfig = new TemplateServiceConfiguration
                {
                    TemplateManager = new DelegateTemplateManager(controllerAndAction => usingHeader + System.IO.File.ReadAllText(System.IO.Path.Combine(_conventionFolderPath, controllerAndAction)) /*+ getResult*/),
                    DisableTempFileLocking = true,
                    CachingProvider = new DefaultCachingProvider(t => { })
                };
                var actionController = $"{controller}/{action}.cshtml";

                var engine = RazorEngineService.Create(templateConfig);


                var html = engine.RunCompile(actionController, request.GetType(), request);

                html = html.Replace("\n", "");
                html = html.Replace("\r", "");
                //html = Framework.Helper.Base64Decode(html);


                return Ok(html);
            }
            catch (Exception ex)
            {
                //return BadRequest(ex.Message);
                return Ok(new Response { IsException = true, Message = ex.Message });

            }

        }

        [HttpPost]
        public async Task<IActionResult> ExecAPI(Request request)
        {

            try
            {
                string controller = "Custom";
                string action = request.View;
                var actionController = $"{controller}/{action}";


                string viewPath = System.IO.Path.Combine(_conventionFolderPath, actionController);


                var html = await _renderer.RenderToStringAsync(actionController, request);
                
                //engine.RunCompile(actionController, request.GetType(), request);

                html = html.Replace("\n", "");
                html = html.Replace("\r", "");
                //html = Framework.Helper.Base64Decode(html);


                return Ok(html);
            }
            catch (Exception ex)
            {
                //return BadRequest(ex.Message);
                return Ok(new Response { IsException = true, Message = ex.Message });

            }

        }



        [HttpGet]
        public ActionResult Prueba()
        {
            return Ok("ServicesController is running.");
        }
    }
}
