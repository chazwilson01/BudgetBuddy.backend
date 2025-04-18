using FinanceTracker.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FinanceTracker.API.Notis;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using Microsoft.Extensions.DependencyInjection;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();


var connectionString = builder.Configuration.GetConnectionString("Default");

builder.Logging.ClearProviders();        // Optional: clears default
builder.Logging.AddConsole();            // Enables console output via ILogger
builder.Logging.SetMinimumLevel(LogLevel.Information); // Optional: ensure INFO logs show up

// Add services to the container
builder.Services.AddHttpClient<PlaidService>();
builder.Services.AddScoped<PlaidService>();


builder.Services.AddControllers(); // Enable attribute routing


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IPasswordService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();


// Add response compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Look for token in cookie
                context.Token = context.HttpContext.Request.Cookies["jwt"];
                return Task.CompletedTask;
            }
        };

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

// CORS: allow React frontend (adjust origin for production)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact",
        policy => policy.WithOrigins("http://localhost:5173", "https://white-sea-0314caa0f.6.azurestaticapps.net", "https://budgetbuddy-780a2.web.app") // React dev server
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

////  Supabase setup
//var supabaseUrl = builder.Configuration["Supabase:Url"];
//var supabaseKey = builder.Configuration["Supabase:Key"];


//var supabaseOptions = new SupabaseOptions
//{
//    AutoConnectRealtime = true
//};

//var supabaseClient = new Supabase.Client(supabaseUrl, supabaseKey, supabaseOptions);
//supabaseClient.InitializeAsync().GetAwaiter().GetResult();

//builder.Services.AddSingleton(supabaseClient);

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Using connection string: {ConnectionString}", connectionString);
logger.LogInformation("CLIENT ID: {ClientId}", builder.Configuration["Plaid:ClientId"]);
logger.LogInformation("PLAID SECRET: {Secret}", builder.Configuration["Plaid:Secret"]);



// Enable Swagger for API testing in dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware
app.UseHttpsRedirection();
app.UseResponseCompression(); // Enable response compression
app.UseCors("AllowReact"); // enable CORS
app.UseAuthentication();
app.UseAuthorization();

// Add security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    await next();
});

app.MapControllers(); // enable attribute routing (e.g., PlaidController)

app.Run();
