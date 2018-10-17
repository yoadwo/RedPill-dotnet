using Marvel.Api;
using Marvel.Api.Filters;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;

namespace RedPill_dotnet.Controllers
{
    public class HomeController : Controller
    {

        readonly String pb_key = "a1886e5a6add5e87b64f9f9eb78bd7aa";
        readonly String pr_key = "1ac327e1f195ee400c9b3b3d256ead0657dba7ea";
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";
            System.Diagnostics.Trace.WriteLine("In Home Controller");

            // accessing developer.marvel.com

            //HTTP GET
            // http://gateway.marvel.com/v1/public/comics?ts=1&apikey=1234&hash=ffd275c5130566a2916217b101f26150


            // randomize which of the 50 retrieved characters will be displayed
            int characterIndex = new Random().Next(0, 99);
            

            var marvelClient = new MarvelRestClient(pb_key, pr_key);
            var filter = new CharacterRequestFilter
            {
                Limit = 100
            };

            var response = marvelClient.FindCharacters(filter);
            var imageUrl = response.Data.Results[characterIndex].Thumbnail.Path + "." +
                response.Data.Results[characterIndex].Thumbnail.Extension;
            if (imageUrl.Contains("image_not_available"))
                imageUrl = "https://www.writeups.org/wp-content/uploads/Spider-Man-Marvel-Comics-Peter-Parker-Profile.jpg";
            ViewBag.imageUrl = imageUrl;


            return View();
        }

    }
}
