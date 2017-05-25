namespace Snow.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.ServiceModel.Syndication;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using JsonFeedNet;
    using Models;
    using Nancy;
    using Nancy.IO;

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
            var items = new List<FeedItem>();

            foreach (var post in model)
            {
                // Replace all relative urls with full urls.
                var contentHtml = Regex.Replace(post.Content, UrlRegex, m => siteUrl.TrimEnd('/') + "/" + m.Value.TrimStart('/'));

                var item = new FeedItem
                {
                    Id = $"{siteUrl}{post.Url}",
                    Url = $"{siteUrl}{post.Url}",
                    Title = post.Title,
                    ContentHtml = contentHtml,
                    DatePublished = post.Date.ToUniversalTime(),
                    DateModified = post.Date.ToUniversalTime(),
                    Author = new Author { Name = this.author, },
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
                //Author TBD once I fix the library.
                Expired = false,
                Items = items
            };

            return stream =>
            {
                var encoding = new UTF8Encoding(false);
                var streamWrapper = new UnclosableStreamWrapper(stream);

                using (var writer = new StreamWriter(streamWrapper, encoding))
                {
                    writer.Write(feed);
                }
            };
        }
    }
}