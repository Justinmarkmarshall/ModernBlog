# Connection String Configuration

This project uses local configuration files to manage connection strings securely.

## Development Setup

The connection strings are stored in `appsettings.Development.json` which is **not committed to source control**.

### Initial Setup

When you first clone this repository, you need to create your local `appsettings.Development.json` file:

1. Navigate to `src/ModernBlog.Web/`
2. The file should already exist with default SQLite connection strings
3. Modify the connection strings if needed for your local environment

### Default Connection Strings

```json
{
  "ConnectionStrings": {
	"DefaultConnection": "DataSource=Data/app.db;Cache=Shared",
  }
}
```
## Production Deployment

For production environments:

### Option 1: Environment Variables
Set these environment variables on your server:
```
ConnectionStrings__DefaultConnection=<your-production-connection-string>
ConnectionStrings__BlogConnection=<your-production-connection-string>
```

### Option 2: Azure App Service Configuration
Add connection strings in the Azure Portal under:
- Configuration → Connection strings

### Option 3: appsettings.Production.json
Create an `appsettings.Production.json` (also not committed to source control) on your production server.

## Security Notes

⚠️ **Never commit these files to source control:**
- `appsettings.Development.json`
- `appsettings.Production.json`
- Any `appsettings.*.json` files (except `appsettings.json`)
- `*.db` files

✅ These files are already in `.gitignore` to prevent accidental commits.
