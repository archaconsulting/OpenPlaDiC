using Microsoft.AspNetCore.Mvc;
using OpenPlaDiC.Framework;
using System.Threading.Tasks;

namespace OpenPlaDiC.WebApp.Controllers
{
    [Route("[controller]")]
    public class CustomController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("{viewName}")]
        public IActionResult HandleUnknownActionGet(string viewName)
        {
            var lista = new List<GlobalItem>();

            foreach (var k in Request.Query)
            {
                lista.Add(new GlobalItem(k.Key, k.Value.ToString() ?? ""));

            }

            return View(viewName,lista);

        }

        [HttpPost("{viewName}")]
        public IActionResult HandleUnknownActionPost(string viewName)
        {
            var lista = new List<GlobalItem>();

            if (HttpContext.Request.Method.ToUpper() == "POST")
            {

                foreach (var item in Request.Form)
                {
                    lista.Add(new GlobalItem(item.Key, item.Value));
                }

            }
            
            return View(viewName, lista);

        }


    }
}
