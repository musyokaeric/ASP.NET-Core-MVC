using DI_Service_Lifetime.Models;
using DI_Service_Lifetime.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;

namespace DI_Service_Lifetime.Controllers
{
    public class HomeController : Controller
    {
        private readonly IScopedGuidService scoped1;
        private readonly IScopedGuidService scoped2;
        private readonly ISingletonGuidService singleton1;
        private readonly ISingletonGuidService singleton2;
        private readonly ITransientGuidService transient1;
        private readonly ITransientGuidService transient2;

        public HomeController(
            IScopedGuidService scoped1, IScopedGuidService scoped2,
            ISingletonGuidService singleton1, ISingletonGuidService singleton2,
            ITransientGuidService transient1, ITransientGuidService transient2)
        {
            this.scoped1 = scoped1;
            this.scoped2 = scoped2;
            this.singleton1 = singleton1;
            this.singleton2 = singleton2;
            this.transient1 = transient1;
            this.transient2 = transient2;
        }

        public IActionResult Index()
        {
            StringBuilder message = new StringBuilder();

            // Transient service lifetime: New service is created every single time
            // Different Guids will be displayed on both transients every time the page is loaded/refreshed
            message.AppendLine($"Transient 1 : {transient1.GetGuid()}");
            message.AppendLine($"Transient 2 : {transient2.GetGuid()} \n");

            // Scoped service lifetime: New service is created once per request
            // Same Guid will be displaged on both scopes
            // Guid will change every time the page is refreshed
            message.AppendLine($"Scoped 1 : {scoped1.GetGuid()}");
            message.AppendLine($"Scoped 2 : {scoped2.GetGuid()} \n");

            // Singleton service lifetime: New service is created once per application lifetime
            // Same Guid will be displaged on both scopes
            // Guid will change when the page is restarted
            message.AppendLine($"Singleton 1 : {singleton1.GetGuid()}");
            message.AppendLine($"Singleton 2 : {singleton2.GetGuid()} \n");

            return Ok(message.ToString());
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