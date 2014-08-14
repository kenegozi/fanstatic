using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Fanstatic.Engine.Processors.Tags;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace Fanstatic.Engine.Processors.Posts
{
    public class CopyProcessor : Processor
    {
        public interface ICopyProcessorSettings
        {
            string[] Items { get; } 
        }

        static bool IsDirectory(string path)
        {
            return (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;
        }

        public override void ExecuteSecondPass(JToken processorSettings, IGeneratorSettings globalSettings, IList<IProcessor> processors)
        {
            var settings = processorSettings.ToObject<ICopyProcessorSettings>(JsonSerializer);
            if (settings.Items == null)
            {
                return;
            }
            if (settings.Items.Length == 0)
            {
                return;
            }
            var targetRoot = Path.Combine(globalSettings.Root, "wwwroot");
            foreach (var item in settings.Items)
            {
                var itemPath = Path.Combine(globalSettings.Root, item);
                if (Directory.Exists(itemPath))
                {
                    foreach (var sourcePath in Directory.EnumerateFileSystemEntries(itemPath, "*", SearchOption.AllDirectories))
                    {
                        var targetPath = Path.Combine(targetRoot, sourcePath.Substring(itemPath.Length).Trim('\\'));
                        if (IsDirectory(sourcePath))
                        {
                            Directory.CreateDirectory(targetPath);
                            Console.WriteLine("Created directory "+targetPath);
                        }
                        else
                        {
                            File.Copy(sourcePath, targetPath, true);
                            Console.WriteLine("copied file " + targetPath);
                        }
                    }
                }
            }
        }
    }
    public class AtomProcessor : Processor
    {

    }
    public class SitemapProcessor : Processor
    {

    }
    public class ArchiveProcessor : Processor
    {

    }
    public class PagesProcessor : Processor
    {

    }

    public class PostsProcessor : Processor
    {
        private readonly IList<Post> posts = new List<Post>();
        const string DefaultPostsLocation = "_posts";
            
        public override void ExecuteFirstPass(JToken processorSettings, IGeneratorSettings globalSettings, IList<IProcessor> previousProcessors)
        {
            var typedProcessorSettings = processorSettings.ToObject<IPostsProcessorSettings>(JsonSerializer);
            var locationItem = typedProcessorSettings.Location ?? DefaultPostsLocation;

            var postsFolder = locationItem;
            if (!Path.IsPathRooted(locationItem))
            {
                postsFolder = Path.Combine(globalSettings.Root, postsFolder);
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

        public override void ExecuteSecondPass(JToken processorSettings, IGeneratorSettings globalSettings, IList<IProcessor> processors)
        {
            var typedProcessorSettings = processorSettings.ToObject<IPostsProcessorSettings>(JsonSerializer);
            var tags = processors.OfType<TagsProcessor>().Single().GetTags();

            foreach (var post in GetPosts())
            {
                var target = post.Permalink.Replace('/', '\\');
                target = Path.Combine(globalSettings.Root, "wwwroot", target);
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