using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Fanstatic.Engine.Processors.Posts;
using Newtonsoft.Json.Linq;

namespace Fanstatic.Engine.Processors.Tags
{
    public class TagsProcessor : Processor
    {
        private readonly IDictionary<string, IList<Post>> tags = new Dictionary<string, IList<Post>>();

        public override void ExecuteFirstPass(JObject processorSettings, IGeneratorSettings globalSettings, IList<IProcessor> previousProcessors)
        {
            var typedProcessorSettings = processorSettings.ToObject<ITagsProcessorSettings>(JsonSerializer);

            var posts = previousProcessors.OfType<PostsProcessor>().Single().GetPosts();

            BuildTags(posts);
        }

        public override void ExecuteSecondPass(JObject processorSettings, IGeneratorSettings globalSettings, IList<IProcessor> processors)
        {
            var typedProcessorSettings = processorSettings.ToObject<ITagsProcessorSettings>(JsonSerializer);
            var permalinkFormat = typedProcessorSettings.PermalinkTemplate ?? "tags/{tag}";
            foreach (var tag in tags)
            {
                Console.WriteLine("tag '{0}' permalink is: {1}", tag.Key, permalinkFormat.Replace("{tag}", tag.Key));
            }
            base.ExecuteSecondPass(processorSettings, globalSettings, processors);
        }

        public IReadOnlyDictionary<string, IList<Post>> GetTags()
        {
            return new ReadOnlyDictionary<string, IList<Post>>(tags);
        }

        private void BuildTags(IEnumerable<Post> posts)
        {
            foreach (var post in posts)
            {
                foreach (var tag in post.Tags)
                {
                    IList<Post> postsInTag;
                    if (!tags.TryGetValue(tag, out postsInTag))
                    {
                        postsInTag = tags[tag] = new List<Post>();
                    }
                    postsInTag.Add(post);
                }
            }
        }
    }
}