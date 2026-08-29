using Microsoft.EntityFrameworkCore;
using ModernBlog.Core.Posts;

namespace ModernBlog.Core.Persistence
{
    public sealed class BlogDbContext(
    DbContextOptions<BlogDbContext> options)
    : DbContext(options)
    {
        public DbSet<Post> Posts => Set<Post>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Post>(post =>
            {
                post.ToTable("posts");

                post.HasKey(x => x.Id);

                post.Property(x => x.Title)
                    .HasMaxLength(200)
                    .IsRequired();

                post.Property(x => x.Slug)
                    .HasMaxLength(220)
                    .IsRequired();

                post.HasIndex(x => x.Slug)
                    .IsUnique();

                post.Property(x => x.Markdown)
                    .IsRequired();

                post.Property(x => x.SeoTitle)
                    .HasMaxLength(70);

                post.Property(x => x.SeoDescription)
                    .HasMaxLength(160);

                post.HasIndex(x => new
                {
                    x.Status,
                    x.PublishedAtUtc
                });
            });
        }
    }
}
