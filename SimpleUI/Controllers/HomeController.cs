using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Serilog;
using SimpleUI.Models;

namespace SimpleUI.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            var x = User;

            Log.Information("We got here....");
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult BadPage(int id)
        {
            ViewData["Message"] = "Your exception page.";
            throw new System.Exception("Craziness!!!");
            //return View();
        }

        public async Task<IActionResult> GoodApi()
        {
            var client = new HttpClient();
            var token = await HttpContext.GetTokenAsync("access_token");
            client.SetBearerToken(token);

            var content = await client.GetStringAsync("https://localhost:44389/api/Values");

            ViewBag.Json = JArray.Parse(content).ToString();
            return View();
        }

        public async Task<IActionResult> UnauthApi()
        {
            var client = new HttpClient();
            var token = await HttpContext.GetTokenAsync("id_token");
            client.SetBearerToken(token);

            var content = await client.GetStringAsync("https://localhost:44389/api/Values");

            ViewBag.Json = JArray.Parse(content).ToString();
            return View("BadApi");  // should never really get here....            
        }

        public async Task<IActionResult> BadApi()
        {
            var client = new HttpClient();
            var token = await HttpContext.GetTokenAsync("access_token");
            client.SetBearerToken(token);

            var content = await client.GetStringAsync("https://localhost:44389/api/Values/123");  // this throws exception

            ViewBag.Json = JArray.Parse(content).ToString();
            return View(); // should never really get here....
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
