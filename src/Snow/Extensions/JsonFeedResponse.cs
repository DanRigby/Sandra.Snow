namespace Snow.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using JsonFeedNet;
    using Models;
    using Nancy;

    public class JsonFeedResponse : Response
    {
        private const string UrlRegex = @"(?<=<(a|img)\s+[^>]*?(href|src)=(?<q>['""]))(?!https?://)(?<url>/?.+?)(?=\k<q>)";
        private readonly string siteUrl;
        private readonly string feedfileName;
        private readonly string author;
        private readonly string feedTitle;

        public JsonFeedResponse(IEnumerable<Post> model, string feedTitle, string siteUrl, string author, string feedfileName)
        {
            this.siteUrl = siteUrl;
            this.feedfileName = feedfileName;
            this.author = author;
            this.feedTitle = feedTitle;

            Contents = GetContents(model);
            ContentType = "application/json";
            StatusCode = HttpStatusCode.OK;
        }

        private Action<Stream> GetContents(IEnumerable<Post> model)
        {
            var items = new List<JsonFeedItem>();

            foreach (var post in model)
            {
                // Replace all relative urls with full urls.
                var contentHtml = Regex.Replace(post.Content, UrlRegex, m => siteUrl.TrimEnd('/') + "/" + m.Value.TrimStart('/'));

                var item = new JsonFeedItem
                {
                    Id = $"{siteUrl}{post.Url}",
                    Url = $"{siteUrl}{post.Url}",
                    Title = post.Title,
                    ContentHtml = contentHtml,
                    DatePublished = post.Date.ToUniversalTime(),
                    DateModified = post.Date.ToUniversalTime(),
                    Author = new JsonFeedAuthor { Name = this.author, },
                    Tags = post.Categories.ToList()
                };

                items.Add(item);
            }

            var feed = new JsonFeed
            {
                Version = @"https://jsonfeed.org/version/1",
                Title = feedTitle,
                HomePageUrl = siteUrl,
                FeedUrl = $"{siteUrl}/{feedfileName}",
                Author = new JsonFeedAuthor { Name = this.author, },
                Expired = false,
                Items = items
            };

            return stream => { feed.Write(stream); };
        }
    }
}