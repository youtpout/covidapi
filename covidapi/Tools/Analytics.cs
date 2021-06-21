using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace covidapi.Tools
{
    public class Analytics
    {
        private IConfiguration conf;
        public Analytics(IConfiguration conf)
        {
            this.conf = conf;
        }

        public async Task Send(string eventName, string eventValue)
        {
            HttpClient http = new HttpClient()
            {
                BaseAddress = new Uri("http://www.google-analytics.com/")
            };
            var content = new FormUrlEncodedContent(new Dictionary<string, string>() {
                        { "v" , "1" },  // API Version.
                        { "tid" , conf["GaTrackingId"] },  // Tracking ID / Property ID.
                        // Anonymous Client Identifier. Ideally, this should be a UUID that
                        // is associated with particular user, device, or browser instance.
                        { "cid" , new Guid().ToString() },
                        { "t" , "event" },  // Event hit type.
                        { "ec" , "Poker" },  // Event category.
                        { "ea" , "Royal Flush" },  // Event action.
                        { "el" , "Hearts" },  // Event label.
                        { "ev" , "0" },  // Event value, must be an integer
                });
            var post = await http.PostAsync("collect", content);
            if (post.IsSuccessStatusCode)
            {
                Debug.WriteLine("ok analytics");
            }
            else
            {
                Debug.WriteLine("false analytics");
            }
        }

        public async Task Get()
        {
            HttpClient http = new HttpClient()
            {
                BaseAddress = new Uri("https://www.google-analytics.com/debug/collect?tid=fake&v=1")
            };

            var post = await http.GetStringAsync("https://www.google-analytics.com/debug/collect?t=pageview&dp=%2FpageA&v=1&cid=32&tid=" + conf["GaTrackingId"]);
            Debug.WriteLine(post);

        }
    }
}
