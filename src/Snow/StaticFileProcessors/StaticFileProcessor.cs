namespace Snow.StaticFileProcessors
{
    using System;
    using System.IO;
    using CsQuery.ExtensionMethods;
    using Extensions;
    using Nancy.Testing;

    public class StaticFileProcessor : BaseProcessor
    {
        public override string ProcessorName
        {
            get { return ""; }
        }
        
        protected override void Impl(SnowyData snowyData, SnowSettings settings)
        {
            TestModule.GeneratedUrl = settings.SiteUrl + "/" + DestinationName.Trim(new[] {'/'}) + "/";

            var result = snowyData.Browser.Post("/static");

            result.ThrowIfNotSuccessful(SourceFile);

            if (!Directory.Exists(Destination))
            {
                Directory.CreateDirectory(Destination);
            }

            var minifier = new WebMarkupMin.Core.Minifiers.HtmlMinifier();

            string pageBody = result.Body.AsString();
            var minificationResult = minifier.Minify(pageBody);

            minificationResult.Errors.ForEach(x => Console.WriteLine(x.Message));
            minificationResult.Warnings.ForEach(x => Console.WriteLine(x.Message));

            File.WriteAllText(Path.Combine(Destination, "index.html"), minificationResult.MinifiedContent);
        }
    }
}