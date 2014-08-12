using System;

namespace Fanstatic.Engine.Processors.Posts
{
    public interface ISpecificPostSettings
    {
        string Permalink { get; }
        string Slug { get; set; }
        DateTime? PublishDate { get; set; }
        string Title { get; set; }
        string[] Tags { get; }
        bool? IsDraft { get; }
    }
}