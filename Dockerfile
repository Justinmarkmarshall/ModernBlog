FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first for better restore layer caching.
COPY ["src/ModernBlog.Web/ModernBlog.Web.csproj", "src/ModernBlog.Web/"]
COPY ["src/ModernBlog.Core/ModernBlog.Core.csproj", "src/ModernBlog.Core/"]
RUN dotnet restore "src/ModernBlog.Web/ModernBlog.Web.csproj"

# Copy source and publish the web app.
COPY . .
WORKDIR /src/src/ModernBlog.Web
RUN dotnet publish "ModernBlog.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# Use a non-root user and keep SQLite files in a writable folder.
RUN mkdir -p /app/Data \
	&& if ! getent group app >/dev/null; then groupadd --gid 10001 app; fi \
	&& if ! id -u app >/dev/null 2>&1; then useradd --uid 10001 --gid app --create-home --shell /usr/sbin/nologin app; fi \
	&& chown -R app:app /app

USER app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ModernBlog.Web.dll"]
