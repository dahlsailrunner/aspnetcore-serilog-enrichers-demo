using System;
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

        public IActionResult BadPageWithQuery(int id, string code)
        {
            throw new Exception("Something bad happened.");
        }

        public async Task<IActionResult> GoodApi()
        {
            var client = new HttpClient();
            var token = await HttpContext.GetTokenAsync("access_token");
            client.SetBearerToken(token);

            var response = await GetWithHandlingAsync(client, "https://localhost:44389/api/Values");
            
            ViewBag.Json = JArray.Parse(await response.Content.ReadAsStringAsync()).ToString();

            return View();
        }

        public async Task<IActionResult> UnauthApi()
        {
            var client = new HttpClient();
            var token = await HttpContext.GetTokenAsync("id_token");  // consciously getting wrong token here
            client.SetBearerToken(token);

            var response = await GetWithHandlingAsync(client, "https://localhost:44389/api/Values");
            
            ViewBag.Json = JArray.Parse(await response.Content.ReadAsStringAsync()).ToString();
            return View("BadApi");  // should never really get here....            
        }

        public async Task<IActionResult> BadApi()
        {
            var client = new HttpClient();
            var token = await HttpContext.GetTokenAsync("access_token");
            client.SetBearerToken(token);

            var response = await GetWithHandlingAsync(client, "https://localhost:44389/api/Values/123");
            
            ViewBag.Json = JArray.Parse(await response.Content.ReadAsStringAsync()).ToString();
            return View(); // should never really get here....
        }        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private static async Task<HttpResponseMessage> GetWithHandlingAsync(HttpClient client, string apiRoute)
        {
            var response = await client.GetAsync(apiRoute);
            if (!response.IsSuccessStatusCode)
            {
                string error = "";
                string id = "";

                if (response.Content.Headers.ContentLength > 0)
                {
                    var j = JObject.Parse(await response.Content.ReadAsStringAsync());
                    error = (string) j["error"];
                    id = (string) j["id"];
                }
                //below logs warning with these details and THEN throws excpetion, which will also get logged
                //    but without the details from the API call and response.
                //    An alternative would be to use Serilog.Enrichers.Exceptions and include the API details
                //    in the ex.Data fields -- e.g. ex.Data.Add("ApiStatus", (int) response.StatusCode);
                //    Then you would throw the exception and only get ONE log entry with all of the details
                var ex = new Exception("API Failure");

                ex.Data.Add("API Route", $"GET {apiRoute}");
                ex.Data.Add("API Status", (int) response.StatusCode);
                if (!string.IsNullOrEmpty(error))
                {
                    ex.Data.Add("API Error", error);
                    ex.Data.Add("API ErrorId", id);
                }
                //Log.Warning(ex,
                //    "Got non-success response from API {ApiStatus}--{ApiError}--{ApiErrorId}--{ApiUrl}",
                //    (int) response.StatusCode,
                //    error,
                //    id,
                //    $"GET {apiRoute}");

                throw ex;
            }            

            return response;
        }
    }
}
