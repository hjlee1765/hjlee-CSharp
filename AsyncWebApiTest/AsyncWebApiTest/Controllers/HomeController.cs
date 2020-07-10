using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using System.Net;
using System.Diagnostics;
using System.Threading;

namespace AsyncWebApiTest.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index() {

			return Content("hello world");
		}
		[HttpGet]
		public async Task<String> Main()
		{
			var res = await new HomeBiz().getAsync();

			Debug.WriteLine("exit - Main" + Thread.CurrentThread.ManagedThreadId);

			return res;
		}
	}

	public class HomeBiz
	{
		public async Task<string> getAsync()
		{
			ServicePointManager.Expect100Continue = true;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

			var client = new HttpClient();
			Task<string> getStringTask = client.GetStringAsync("http://docs.microsoft.com/dotnet");

			string urlContents = await getStringTask;

			Debug.WriteLine("exit - AccessTheWebAsnyc()" + Thread.CurrentThread.ManagedThreadId);
			return urlContents.Length.ToString();
		}
	}

}