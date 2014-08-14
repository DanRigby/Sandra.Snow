namespace Snow.StaticFileProcessors
{
    using CsQuery.ExtensionMethods;
    using Extensions;
    using Nancy.Testing;
    using System;
    using System.IO;
    using System.Linq;

    public class PostsProcessor : BaseProcessor
    {
        public override string ProcessorName
        {
            get { return "posts"; }
        }

        protected override void Impl(SnowyData snowyData, SnowSettings settings)
        {

            var filteredPosts = snowyData.Files.Where(ShouldProcess.Posts).ToList();

            var pageSize = settings.PageSize;
            var skip = 0;
            var iteration = 1;
            var currentIteration = filteredPosts.Skip(skip).Take(pageSize).ToList();
            var totalPages = (int)Math.Ceiling((double)filteredPosts.Count / pageSize);

            TestModule.TotalPages = totalPages;

            while (currentIteration.Any())
            {
                var folder = skip <= 1 ? "" : "page" + iteration;

                TestModule.PostsPaged = currentIteration.ToList();
                TestModule.PageNumber = iteration;
                TestModule.HasNextPage = iteration < totalPages;
                TestModule.HasPreviousPage = iteration > 1 && totalPages > 1;
                TestModule.GeneratedUrl = (settings.SiteUrl + "/" + folder).TrimEnd('/') + "/";

                var result = snowyData.Browser.Post("/static");

                result.ThrowIfNotSuccessful(snowyData.File.File);

                var outputFolder = Path.Combine(snowyData.Settings.Output, folder);

                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }

                var minifier = new WebMarkupMin.Core.Minifiers.HtmlMinifier();

                string pageBody = result.Body.AsString();
                var minificationResult = minifier.Minify(pageBody);

                minificationResult.Errors.ForEach(x => Console.WriteLine(x.Message));
                minificationResult.Warnings.ForEach(x => Console.WriteLine(x.Message));

                File.WriteAllText(Path.Combine(outputFolder, "index.html"), minificationResult.MinifiedContent);

                skip += pageSize;
                iteration++;
                currentIteration = filteredPosts.Skip(skip).Take(pageSize).ToList();
            }
        }
    }
}