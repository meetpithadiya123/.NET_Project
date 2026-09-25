using E_Commerce_Website.Models;
using E_Commerce_Website.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// Register Mailtrap SMTP & HTTP API Email Service
builder.Services.AddScoped<IEmailService, MailtrapEmailService>();

builder.Services.AddDbContext<Mycontext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("myconnection")));

// High-speed response compression (Brotli + Gzip)
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "image/svg+xml", "application/json", "text/css", "application/javascript" });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

// Session Configuration with Security Attributes
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(90);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// Antiforgery Cookie Security
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// Rate limiting to defend against brute force & DoS attacks
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 120, // 120 requests per minute per IP
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Enable response compression early in pipeline
app.UseResponseCompression();

// Security Headers Middleware (Defends against XSS, Clickjacking, MIME sniffing)
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    context.Response.Headers.Remove("Server");
    context.Response.Headers.Remove("X-Powered-By");

    await next();
});

app.UseHttpsRedirection();

// Rate limiter middleware
app.UseRateLimiter();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

// Optimized static files with browser caching
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=2592000,stale-while-revalidate=86400");
    }
});

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=customer}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
