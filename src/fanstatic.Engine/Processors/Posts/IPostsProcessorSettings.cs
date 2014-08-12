using System.ComponentModel;

namespace Fanstatic.Engine.Processors.Posts
{
    public interface IPostsProcessorSettings
    {
        string PermalinkTemplate { get; }

        string Location { get; }

        string Permalink { get; }
    }
}