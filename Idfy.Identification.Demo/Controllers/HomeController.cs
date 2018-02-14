using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Idfy.Demo.Common;
using Idfy.Identification.Client;
using Idfy.Identification.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Idfy.Identification.Demo.Controllers
{
    public class HomeController : Controller
    {
        private Idfy.Identification.Client.IdentificationClient client;
        private string baseUrl;
        private MemoryCache cache;

        public HomeController()
        {
            client= new IdentificationClient(Credentials.ClientId, Credentials.ClienttSecret,environment:IdentificationClient.Environment.TEST);
            cache=MemoryCache.Default;
            baseUrl = "https://localhost:44335";
        }
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> About()
        {
            ViewBag.Message = "Your application description page.";

            var result = await client.Create(new CreateIdentificationRequest()
            {
                iFrame = new iFrameSettings()
                {
                    Domain = "localhost:44335",                    
                },
                ExternalReference = Guid.NewGuid().ToString("n"),
                ReturnUrls = new ReturnUrls()
                {
                    Abort = baseUrl+Url.Action("Contact")+ "?sessionId=[1]&externalReference=[2]",
                    Error = baseUrl + Url.Action("Contact") + "?sessionId=[1]&externalReference=[2]&statusCode=[0]",
                    Success = baseUrl + Url.Action("Contact") + "?sessionId=[1]&externalReference=[2]",
                }
            });

            ViewBag.Url = result.Url;
            ViewBag.SessionId = result.RequestId;

            return View();
        }

        public async Task<ActionResult>  Contact(string sessionId,string externalReference, string statusCode="LOGIN")
        {
            ViewBag.Message = "Your contact page.";
            var result = await client.GetResponse(sessionId, true);

            ViewBag.Json = Newtonsoft.Json.JsonConvert.SerializeObject(result, Formatting.Indented,
                new JsonConverter[] {new StringEnumConverter()});
            return View();
        }
    }
}