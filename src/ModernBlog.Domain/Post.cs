namespace ModernBlog.Domain.Posts;

public sealed class Post
{
    private Post()
    {
    }

    public Post(
        string title,
        string slug,
        string markdown,
        string authorId)
    {
        Id = Guid.CreateVersion7();
        SetTitle(title);
        SetSlug(slug);
        SetContent(markdown);

        AuthorId = authorId;
        Status = PostStatus.Draft;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Markdown { get; set; } = string.Empty;

    public string? Excerpt { get; set; }

    public string? SeoTitle { get; set; }

    public string? SeoDescription { get; private set; }

    public string AuthorId { get; private set; } = string.Empty;

    public PostStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public DateTimeOffset? PublishedAtUtc { get; private set; }

    public void Update(
        string title,
        string slug,
        string markdown,
        string? excerpt,
        string? seoTitle,
        string? seoDescription)
    {
        SetTitle(title);
        SetSlug(slug);
        SetContent(markdown);

        Excerpt = excerpt?.Trim();
        SeoTitle = seoTitle?.Trim();
        SeoDescription = seoDescription?.Trim();
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Publish()
    {
        Status = PostStatus.Published;
        PublishedAtUtc ??= DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void ReturnToDraft()
    {
        Status = PostStatus.Draft;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private void SetTitle(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        Title = title.Trim();
    }

    private void SetSlug(string slug)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        Slug = slug.Trim().ToLowerInvariant();
    }

    private void SetContent(string markdown)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(markdown);

        Markdown = markdown;
    }
}

public enum PostStatus
{
    Draft = 0,
    Published = 1,
    Archived = 2
}