using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Fanstatic.Engine.Processors.Tags;
using Newtonsoft.Json.Linq;

namespace Fanstatic.Engine.Processors.Posts
{
    public class PostsProcessor : Processor
    {
        private readonly IList<Post> posts = new List<Post>();
        const string DefaultPostsLocation = "_posts";
            
        public override void ExecuteFirstPass(JObject processorSettings, IGeneratorSettings globalSettings, IList<IProcessor> previousProcessors)
        {
            var typedProcessorSettings = processorSettings.ToObject<IPostsProcessorSettings>(JsonSerializer);
            var locationItem = typedProcessorSettings.Location ?? DefaultPostsLocation;

            var postsFolder = locationItem;
            if (!Path.IsPathRooted(locationItem))
            {
                postsFolder = Path.Combine(globalSettings.RootDirectory, postsFolder);
            }

            var inputFiles = Directory.EnumerateFiles(postsFolder, "*.markdown", SearchOption.AllDirectories);
            foreach (var inputFile in inputFiles)
            {
                var fileContent = File.ReadAllText(inputFile);
                var fileName = Path.GetFileName(inputFile);
                ISpecificPostSettings frontMatter;
                string content;
                if (fileContent.StartsWith("---"))
                {
                    var endOfFrontMatter = fileContent.IndexOf("---", 3, StringComparison.Ordinal);
                    var frontMatterContent = fileContent.Substring(3, endOfFrontMatter - 3).Trim();
                    frontMatter = JObject.Parse(frontMatterContent).ToObject<ISpecificPostSettings>(JsonSerializer);
                    content = fileContent.Substring(endOfFrontMatter + 3).Trim();
                }
                else
                {
                    content = fileContent;
                    frontMatter = null;
                }
                posts.Add(new Post(fileName, content, globalSettings, typedProcessorSettings, frontMatter));
            }
         }

        public override void ExecuteSecondPass(JObject processorSettings, IGeneratorSettings globalSettings, IList<IProcessor> processors)
        {
            var typedProcessorSettings = processorSettings.ToObject<IPostsProcessorSettings>(JsonSerializer);
            var tags = processors.OfType<TagsProcessor>().Single().GetTags();

            foreach (var post in GetPosts())
            {
                var target = post.Permalink.Replace('/', '\\');
                target = Path.Combine(globalSettings.RootDirectory, "wwwroot", target);
                Directory.CreateDirectory(target);
                var tarfetFile = Path.Combine(target, "index.html");
                var result = RazorEngine.Razor.Run("_posts/Post", post);
                File.WriteAllText(tarfetFile, result);
            }

            foreach (var tag in tags.Keys)
            {
                Console.WriteLine("{0}:\r\n  {1}", tag, string.Join("\r\n  ", tags[tag].Select(p => p.Title)));
                Console.WriteLine();
            }



        }

        public IReadOnlyList<Post> GetPosts()
        {
            return new ReadOnlyCollection<Post>(posts);
        }
    }
}