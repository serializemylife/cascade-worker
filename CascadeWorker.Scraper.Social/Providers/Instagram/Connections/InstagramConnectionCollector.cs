using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using OpenQA.Selenium;

namespace CascadeWorker.Scraper.Social.Providers.Instagram
{
    public static class InstagramConnectionCollector
    {
        public static HttpResponseMessage GetConnectionsFromApi(long profileId, string csrfToken, ReadOnlyCollection<Cookie> cookies, string endCursor)
        {
            var igDid = cookies.First(x => x.Name == "ig_did").Value;
            var mid = cookies.First(x => x.Name == "mid").Value;
            var dsUserId = cookies.First(x => x.Name == "ds_user_id").Value;
            var sessionId = cookies.First(x => x.Name == "sessionid").Value;

            var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };

            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(
                    "https://www.instagram.com/graphql/query/?query_hash=c76146de99bb02f6415203be841dd25a&variables={\"id\":\"" +
                    profileId + "\",\"include_reel\":true,\"fetch_mutual\":false,\"first\":20" +
                    (endCursor == string.Empty ? "}" : ",\"after\":\"" + endCursor + "\"}")),
                Headers =
                {
                    {
                        "cookie",
                        $"ig_did={igDid}; mid={mid}; fbm_124024574287414=base_domain=.instagram.com; shbid=7580; shbts=1597325235.9513137; csrftoken={csrfToken}; ds_user_id={dsUserId}; sessionid={sessionId}; rur=FTW;"
                    },
                    {"x-csrftoken", csrfToken}
                },
            };

            return httpClient.SendAsync(httpRequestMessage).Result;
        }
    }
}