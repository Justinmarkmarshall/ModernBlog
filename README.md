Admin creates draft
    ↓
Draft stored in PostgreSQL
    ↓
Admin publishes it
    ↓
Public route /blog/{slug}
    ↓
Markdown rendered as sanitised HTML

There are 2 DBContexts, the AppDbContext conatins all of the Identity stuff, and the BlogsDbContext contains all the blog stuff.
