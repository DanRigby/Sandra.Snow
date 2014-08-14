namespace Snow.StaticFileProcessors
{
    using System;
    using CsQuery.ExtensionMethods;
    using Extensions;
    using Models;
    using Nancy.Testing;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class CategoriesProcessor : BaseProcessor
    {
        public override string ProcessorName
        {
            get { return "categories"; }
        }

        protected override void Impl(SnowyData snowyData, SnowSettings settings)
        {
            foreach (var tempCategory in TestModule.Categories)
            {
                var category = tempCategory;

                var posts = GetPosts(snowyData.Files, category);

                TestModule.Category = category;
                TestModule.GeneratedUrl = settings.SiteUrl + "/category/" + category.Url + "/";
                TestModule.PostsInCategory = posts.ToList();

                var result = snowyData.Browser.Post("/static");

                result.ThrowIfNotSuccessful(SourceFile);

                var outputFolder = Path.Combine(snowyData.Settings.Output, "category", category.Url);

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
            }
        }

        internal IEnumerable<Post> GetPosts(IList<Post> files, Category category)
        {
            return files.Where(x => x.Categories.Contains(category.Name)).Where(ShouldProcess.Category);
        }
    }
}