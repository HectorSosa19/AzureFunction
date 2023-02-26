using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace FunctionApp2
{
    public class Function1
    {
        [FunctionName("Function1")]
        public async Task Run([TimerTrigger("0 0 0 * * *", RunOnStartup = true )]TimerInfo myTimer, ILogger log)
        {
            if (myTimer.IsPastDue)
                return;
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            List<string> links = new();
            HtmlWeb webScrapt = new();
            HtmlDocument scrapt = webScrapt.Load("https://cve.mitre.org/data/downloads/index.html");

            var csvElementLink = scrapt.DocumentNode
                 .Descendants("a")
                 .First(x => x.InnerHtml.Equals("allitems.csv"));

            string fileUrl = "https://cve.mitre.org" + csvElementLink.Attributes["href"].Value;
            using var client = new HttpClient();
            using var response = await client.GetAsync(fileUrl);

            if (response.IsSuccessStatusCode)
            {
                using var stream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream("allitems.csv", FileMode.Create, FileAccess.Write);
                await stream.CopyToAsync(fileStream);
                Console.WriteLine("File Download Successful");
            }
        }
    }
}
