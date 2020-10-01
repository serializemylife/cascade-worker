using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace CascadeWorker.Scraper.Notifications
{
    public class NotificationSender
    {
        private readonly string _appKey;
        private readonly string _appSecret;

        public NotificationSender(string appKey, string appSecret)
        {
            _appKey = appKey;
            _appSecret = appSecret;
        }
        
        public void SendNotification(string content, string channel, string url = "")
        {
            SendPushedNotification(content, channel, url);
        }

        private void SendPushedNotification(string content, string channel, string url)
        {
            var parameters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("app_key", _appKey),
                new KeyValuePair<string, string>("app_secret", _appSecret),
                new KeyValuePair<string, string>("content", content),
            };
            
            if (!string.IsNullOrEmpty(url))
            {
                parameters.Add(new KeyValuePair<string, string>("content_type", "url"));
                parameters.Add(new KeyValuePair<string, string>("content_extra", url));
            }
	
            if (string.IsNullOrEmpty(channel)) 
            {
                parameters.Add(new KeyValuePair<string, string>("target_type", "app"));
            }
            else 
            {
                parameters.Add(new KeyValuePair<string, string>("target_type", "channel"));
                parameters.Add(new KeyValuePair<string, string>("target_alias", channel));
            }
            
            var responseMessage = GetResponseMessage("https://api.pushed.co/1/push", parameters);
            
            if (!responseMessage.IsSuccessStatusCode)
            {
                Console.WriteLine("{0} ({1})", (int)responseMessage.StatusCode, responseMessage.ReasonPhrase);
            }
        }

        private static HttpResponseMessage GetResponseMessage(string url, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
            client.DefaultRequestHeaders.Add("Accept", "*/*");

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(parameters)
            };

            return client.SendAsync(request).Result;
        }
    }
}