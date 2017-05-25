namespace Snow.StaticFileProcessors
{
    using Models;
    using Nancy.Testing;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class JsonFeedProcessor : BaseProcessor
    {
        public override string ProcessorName
        {
            get { return "jsonfeed"; }
        }

        protected override void Impl(SnowyData snowyData, SnowSettings settings)
        {
            var postsJsonFeed = GetPostsForJsonFeed(snowyData.Files);

            TestModule.PostsPaged = postsJsonFeed;

            var result = snowyData.Browser.Post("/jsonfeed");

            var outputFolder = snowyData.Settings.Output;

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            File.WriteAllText(Path.Combine(outputFolder, SourceFile), result.Body.AsString());
        }

        internal List<Post> GetPostsForJsonFeed(IList<Post> files)
        {
            return files.Where(ShouldProcess.Feed).Take(10).ToList();
        }
    }
}
