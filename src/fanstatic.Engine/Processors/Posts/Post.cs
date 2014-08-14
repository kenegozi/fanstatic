using System;
using System.Linq;
using System.Text.RegularExpressions;
using MarkdownDeep;

namespace Fanstatic.Engine.Processors.Posts
{
    public class Post
    {
        static readonly Regex FilenameHandler = new Regex(@"^(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})-(?<slug>.+)\.(?:md|markdown)", RegexOptions.Compiled);
        static readonly Regex PermalinkTemplateHandler = new Regex(@"{(?<setting>\w+)(?<format>[^}]+)?}", RegexOptions.Compiled);

        private readonly string sourceFileName;
        private readonly IGeneratorSettings generatorSettings;
        private readonly IPostsProcessorSettings postsSettings;
        private readonly ISpecificPostSettings specificPostSettings;

        public Post(string sourceFileName, string content, IGeneratorSettings generatorSettings, IPostsProcessorSettings postsSettings, ISpecificPostSettings specificPostSettings)
        {
            this.sourceFileName = sourceFileName;
            this.generatorSettings = generatorSettings;
            this.postsSettings = postsSettings;
            this.specificPostSettings = specificPostSettings;

            SetSlugAndDate();
            HandlePermalink();

            Content = content;
            PublishDate = ((DateTime?)(GetSetting("date"))).GetValueOrDefault();
            Title = specificPostSettings.Title ?? SlugToTitle();
            Tags = specificPostSettings.Tags;
            IsDraft = specificPostSettings.IsDraft.GetValueOrDefault();

            HtmlContent = new Markdown().Transform(content);
        }

        public string HtmlContent { get; private set; }

        public string Content { get; private set; }

        public string Permalink { get; private set; }

        public DateTime PublishDate { get; private set; }

        public bool IsDraft { get; private set; }

        public string Title { get; private set; }

        public string[] Tags { get; private set; }

        private string SlugToTitle()
        {
            return string.Join(" ", specificPostSettings.Slug.Split('-', '_'));
        }

        private void HandlePermalink()
        {
            var permalink = specificPostSettings.Permalink ?? postsSettings.Permalink ?? "posts/{PublishDate:yyyy}/{PublishDate:MM}/{PublishDate:dd}/{slug}";

            if (permalink.IndexOf('{') > -1)
            {
                permalink = PermalinkTemplateHandler.Replace(permalink, match =>
                {
                    var setting = match.Groups["setting"].Value;
                    var format = match.Groups["format"].Value;
                    var value = GetSetting(setting);
                    var formatString = "{0" + format + "}";
                    var formattedValue = string.Format(formatString, value);
                    return formattedValue;
                });
            }
            Permalink = permalink;
        }

        private object GetSetting(string settingName)
        {
            return GetSetting(settingName, specificPostSettings) ??
                      GetSetting(settingName, postsSettings) ??
                      GetSetting(settingName, generatorSettings);
        }

        private object GetSetting<T>(string settingName, T settings)
        {
            var val = typeof(T).GetProperties().Where(p => p.Name.Equals(settingName, StringComparison.OrdinalIgnoreCase)).Select(p => p.GetValue(settings)).FirstOrDefault();
            return val;
        }

        private void SetSlugAndDate()
        {
            var match = FilenameHandler.Match(sourceFileName);
            if (string.IsNullOrEmpty(specificPostSettings.Slug))
            {
                var slug = match.Groups["slug"].Value;
                specificPostSettings.Slug = slug;
            }
            if (!specificPostSettings.PublishDate.HasValue)
            {
                var year = int.Parse(match.Groups["year"].Value);
                var month = int.Parse(match.Groups["month"].Value);
                var day = int.Parse(match.Groups["day"].Value);
                var date = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
                specificPostSettings.PublishDate = date;
            }
        }
    }
}