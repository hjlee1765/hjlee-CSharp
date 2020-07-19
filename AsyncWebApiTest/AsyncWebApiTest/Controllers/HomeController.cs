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

	/*
	 * API 안에서 여러 시간걸리는 작업(A,B)을 호출해야 한다 하자.
	 * 
	 * A,B를 비동기로 각각 쏘고, whenAll을 하더라도 결국 호출자인 Controller까지 제어권이 넘어오게되며
	 * controller에서 await을 만나면 해당 쓰레드를 중단시키고, 제어권으로 또다른 스레드를 실행시키게 하는걸까?
	 *
	 * -> controller에서 await을 만나면, asp.net request thread pool. 
	 *   즉, 이 서버는 더 많은 request를 처리할수 있게 되는 것이다
	 *
	 *	controller가 async라면 return하기 전, 무조건 await가 필요하다.
	 *	api의 결과를 즉시 리턴하여 view로 먼저 뿌려주고, 오래 걸리는 작업(A,B)의 결과를 받아서 다시 view로 뿌릴 수 없다.
	 *  즉, async/await로 HTTP request-response protocol을 깰 수 없다.
	 *  
	 *  결과를 즉시 리턴시키고 싶다면, background thread에 큐잉을 했다가 client가 주기적으로 체크해서 결과를 가져오는 방식으로 해야될까???
	 *  -> 토비가 언급했었는데 그새 까먹으.. deferredResult 큐 확인해야겠다.
	 *  
	 *  
	 *  참조 : https://stackoverflow.com/questions/31244301/async-await-in-mvc-controllers-action
	 
	 */

	public class HomeController : Controller
	{
		public ActionResult Index() {

			return Content("hello world");
		}

		//Async Controller는 await만나면 ThreadPool을 더 자유롭게 만든다.
		[HttpGet]
		public async Task<String> Main()
		{
			var res = await new HomeBiz().doSomething();

			return res;
		}

		//결과 즉시 리턴시, controller는 async 메서드가 될 수 없다.
		[HttpGet]
		public ActionResult CompleteImmediatelyReturn2()
		{
			var res = new HomeBiz().doSomething();
			return Content("Complete Immediately Return : " + res.ToString());
		}
	}

	public class HomeBiz
	{
		public async Task<string> doSomething()
		{
			Stopwatch stopWatch = new Stopwatch();
			stopWatch.Start();

			var res = getAsync();

			var res2 = getAsync2();
			await Task.WhenAll(res, res2);

			stopWatch.Stop();

			Debug.WriteLine("exec - doSomething / thread : " + Thread.CurrentThread.ManagedThreadId);
			Debug.WriteLine(stopWatch.Elapsed.ToString());
			return res.Result + res2.Result;
			//return res + res2;
		}

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

		public static async Task<string> getAsync2()
		{
			ServicePointManager.Expect100Continue = true;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

			var client = new HttpClient();
			Task<string> getStringTask = client.GetStringAsync("http://docs.microsoft.com/dotnet");

			string urlContents = await getStringTask;

			Debug.WriteLine("exec - getAsync2 / thread : " + Thread.CurrentThread.ManagedThreadId);
			return urlContents.Length.ToString() + "2";
		}
	}

}