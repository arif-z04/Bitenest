using System.Text;
using System.Text.Json.Serialization;
using BiteNest.Api.Data;
using BiteNest.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Controllers with JSON String Enum conversion
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// 2. Configure Database Context (MySQL with development fallback)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var useMySql = builder.Configuration.GetValue<bool>("UseMySql", false);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (useMySql && !string.IsNullOrEmpty(connectionString))
    {
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    }
    else
    {
        options.UseInMemoryDatabase("BiteNestDb");
    }
});

// 3. Register Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IQueueStatusService, QueueStatusService>();
builder.Services.AddScoped<ICombinedDeliveryService, CombinedDeliveryService>();
builder.Services.AddScoped<IMealPlannerService, MealPlannerService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// 4. Configure JWT Authentication
var jwtSecret = builder.Configuration["Jwt:SecretKey"] ?? "BiteNest_Super_Secret_Production_Key_2026_Secure_JWT_Key_Min256Bits";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "BiteNestApi",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "BiteNestUsers",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

builder.Services.AddAuthorization();

// 5. Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 6. Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed initial database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    DbInitializer.Initialize(db);
}

// HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BiteNest API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/", () => Results.Ok(new
{
    System = "BiteNest Smart Food Delivery API",
    Version = "1.0.0",
    Status = "Healthy",
    Swagger = "/swagger",
    Timestamp = DateTime.UtcNow
}));

app.Run();
