using Markdig;

namespace ModernBlog.Core.Markdown;

public interface IMarkdownRenderer
{
    string Render(string markdown);
}

public sealed class MarkdownRenderer : IMarkdownRenderer
{
    private readonly MarkdownPipeline _pipeline =
        new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();

    public string Render(string markdown)
    {
        ArgumentNullException.ThrowIfNull(markdown);

        return Markdig.Markdown.ToHtml(markdown, _pipeline);
    }
}