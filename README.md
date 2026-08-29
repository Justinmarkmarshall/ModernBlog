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


## Deployment

Rancher Desktop Envoy gateway

helm install modernblog .\helm\modernblog --values .\helm\modernblog\values-dev.yaml -n mb

add modernblog.local to hosts file