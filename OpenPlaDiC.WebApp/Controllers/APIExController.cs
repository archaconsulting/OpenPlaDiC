using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenPlaDiC.BIZ;
using OpenPlaDiC.Framework;
using System.Text;
using System.Text.Json;

namespace OpenPlaDiC.WebApp.Controllers
{
    //[Route("api/[controller]/[action]")]

    [Route("[controller]")]
    [ApiController]
    public class APIExController : ControllerBase
    {

        private readonly IDataService _dataService;

        public APIExController(IDataService dataService)
        {
            _dataService = dataService;
        }

        //[HttpGet]
        //public IActionResult Today()
        //{
        //    return Ok(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        //}

        [HttpGet("{*actionName}")] // Catches any GET requests to unknown actions
        [HttpPost("{*actionName}")] // Catches any POST requests to unknown actions
                                    // Add other HTTP methods as needed
        public async Task<IActionResult> HandleUnknownAsync(string actionName)
        {

            string m = HttpContext.Request.Method.ToUpper();

            List<GlobalItem> paramList = new List<GlobalItem>();

            switch (m)
            {
                case "POST":

                    string requestBody;
                    // Enable buffering to allow reading the stream
                    HttpContext.Request.EnableBuffering();

                    // Rewind the stream to the beginning in case other middleware read it
                    HttpContext.Request.Body.Position = 0;

                    using (var reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8))
                    {
                        requestBody = await reader.ReadToEndAsync();
                    }



                    if (!string.IsNullOrEmpty(requestBody))
                    {
                        OpenPlaDiC.Framework.Request request = JsonSerializer.Deserialize<Request>(requestBody);
                        if (request != null && request.Parameters != null)
                        {
                            paramList = request.Parameters;
                        }

                        var respV = await _dataService.GetQueryAsync("select * from ");


                    }



                    break;

                case "GET":
                    break;

                default:
                    break;
            }


            // Log the unknown action attempt if necessary
            // Return a 404 Not Found response
            return NotFound($"Action '{actionName}' / {m} not found.");
        }



    }
}
