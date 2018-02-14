using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Idfy.Demo.Common;

using Idfy.RestClient;
using Idfy.RestClient.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace Idfy.Identification.Demo.Controllers
{
    public class HomeController : Controller
    {
        private IdfyRestClientClient client;
        private string baseUrl;
        private MemoryCache cache;
        private static OAuthToken token;

        public HomeController()
        {
            //client= new IdentificationClient(Credentials.ClientId, Credentials.ClienttSecret,environment:IdentificationClient.Environment.TEST);
            client=new Idfy.RestClient.IdfyRestClientClient(Credentials.ClientId, Credentials.ClienttSecret);
            token = client.Auth.Authorize(scopes: new List<OAuthScope>() { OAuthScope.IDENTIFY });

            cache = MemoryCache.Default;
            cache.Add("token", token, DateTimeOffset.FromUnixTimeSeconds(token.ExpiresIn.Value-10));

            Idfy.RestClient.Configuration.OAuthTokenUpdateCallback = tokenUpdate;

        baseUrl = "https://localhost:44335";
        }

        private void tokenUpdate(OAuthToken oAuthToken)
        {
            if (cache.Contains("token"))
                cache.Remove("token");

            cache.Add("token", token, DateTimeOffset.FromUnixTimeSeconds(token.ExpiresIn.Value - 10));
            
        }

        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> About()
        {
            ViewBag.Message = "Your application description page.";
            client.Auth.UpdateAccessToken(token);

            var result = await client.IdentificationSession.CreateSessionAsync(
                new RestClient.Models.CreateIdentificationRequest()
                {
                    IFrame = new IFrameSettings()
                    {
                        Domain = "localhost:44335",
                    },
                    ExternalReference = Guid.NewGuid().ToString("n"),
                    ReturnUrls = new RestClient.Models.ReturnUrls()
                    {
                        Abort = baseUrl + Url.Action("Contact") + "?sessionId=[1]&externalReference=[2]",
                        Success = baseUrl + Url.Action("Contact") + "?sessionId=[1]&externalReference=[2]",
                        AdditionalProperties = new Dictionary<string, object>()
                        {
                            {"Error", baseUrl + Url.Action("Contact") + "?sessionId=[1]&externalReference=[2]&statusCode=[0]"}
                        }
                        
                    }
                });

            //var result2 = await client.IdentificationSession.CreateSession().Create(new CreateIdentificationRequest()
            //{
            //    iFrame = new iFrameSettings()
            //    {
            //        Domain = "localhost:44335",                    
            //    },
            //    ExternalReference = Guid.NewGuid().ToString("n"),
            //    ReturnUrls = new ReturnUrls()
            //    {
            //        Abort = baseUrl+Url.Action("Contact")+ "?sessionId=[1]&externalReference=[2]",
            //        Error = baseUrl + Url.Action("Contact") + "?sessionId=[1]&externalReference=[2]&statusCode=[0]",
            //        Success = baseUrl + Url.Action("Contact") + "?sessionId=[1]&externalReference=[2]",
            //    }
            //});

            ViewBag.Url = result.Url;
            ViewBag.SessionId = result.RequestId;

            return View();
        }

        public async Task<ActionResult>  Contact(string sessionId,string externalReference, string statusCode="LOGIN")
        {
            ViewBag.Message = "Your contact page.";
            client.Auth.CheckAuthorization();
            var result = await client.IdentificationSession.RetrieveSessionResponseAsync(sessionId,true);

            ViewBag.Json = Newtonsoft.Json.JsonConvert.SerializeObject(result, Formatting.Indented,
                new JsonConverter[] {new StringEnumConverter()});
            return View();
        }
    }
}