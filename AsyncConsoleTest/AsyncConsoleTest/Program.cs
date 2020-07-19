using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncConsoleTest
{

	/*
	1. await는 nonblocking async 방식인가? -> 물론. await 의 호출자에게 제어권을 반환한다.
	2. await 2개가 있을때 아래 await 메서드는 실행이 안되나? -> 첫번째 await에 걸리면 호출자로 제어권이 반환되기에 await아래에 있는 await는 결국
															첫번째 await가 끝나야 두번째 await 메서드가 실행된다. 
	3. whenall 과 waitall 의 차이가 무엇인가? -> whenall : nonblocking 이다. 제어권을 호출자로 넘긴다.
											-> waitall : blocking이다.
	4. task.result는 어디서기다리나? (동기인가 비동기인가?)

	 *  */

	class Program
	{

		public static async Task<string> getAsync()
		{
			ServicePointManager.Expect100Continue = true;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

			var client = new HttpClient();
			Task<string> getStringTask = client.GetStringAsync("http://docs.microsoft.com/dotnet");

			string urlContents = await getStringTask;

			Console.WriteLine("exec - getAsync / thread : " + Thread.CurrentThread.ManagedThreadId);
			return urlContents.Length.ToString();
		}
		public static async Task<string> getAsync2()
		{
			ServicePointManager.Expect100Continue = true;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

			var client = new HttpClient();
			Task<string> getStringTask = client.GetStringAsync("http://docs.microsoft.com/dotnet");
			
			string urlContents = await getStringTask;

			Console.WriteLine("exec - getAsync2 / thread : " + Thread.CurrentThread.ManagedThreadId);
			return urlContents.Length.ToString() + "2";
		}

		public static async Task<string> doSomething()
		{
			Stopwatch stopWatch = new Stopwatch();
			stopWatch.Start();

			var res = getAsync();

			var res2 = getAsync2();

			await Task.WhenAll(res, res2);

			stopWatch.Stop();

			Console.WriteLine("exec - doSomething / thread : " + Thread.CurrentThread.ManagedThreadId);
			Console.WriteLine(stopWatch.Elapsed.ToString());
			return res.Result + res2.Result;
			//return res + res2;
		}

		static void Main(string[] args)
		{
			var res = doSomething();
			//var res = doSomething().GetAwaiter().GetResult();

			Console.WriteLine("exit - main / res : " + res + " thread : " + Thread.CurrentThread.ManagedThreadId);
			Console.ReadLine();
		}
	}
}
