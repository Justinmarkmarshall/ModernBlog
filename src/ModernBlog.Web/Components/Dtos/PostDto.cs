namespace ModernBlog.Web.Components.Dtos
{
    public class PostDto
    {
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Markdown { get; set; }
        public string? Excerpt { get; set; }
        public string? SeoTitle { get; set; }
        public string? SeoDescription { get; set; }
        public string Author { get; set; }

        public string PublishAtUtc { get; set; }
    }
}
