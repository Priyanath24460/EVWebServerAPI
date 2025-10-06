using EVChargingBookingAPI.Data;
using EVChargingBookingAPI.Repositories;
using EVChargingBookingAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// ==================== Configure Services ====================

// Configure MongoDB settings
builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDBSettings"));

// Add MongoDB context
builder.Services.AddSingleton<MongoDBContext>();

// Register Repositories
builder.Services.AddScoped<IEVOwnerRepository, EVOwnerRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IChargingStationRepository, ChargingStationRepository>();

// Register Services
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IChargingStationService, ChargingStationService>();
builder.Services.AddScoped<IQRCodeService, QRCodeService>();

// Add controllers, Swagger, and CORS
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS policy for web and mobile app compatibility
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebAndMobile", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",     // React dev server
                "http://localhost:5173",     // Vite dev server
                "http://localhost:4173",     // Vite preview
                "http://localhost",          // General localhost
                "http://10.0.2.2",          // Android emulator
                "http://192.168.8.179",     // Your local IP
                "https://your-domain.com"   // Your production domain
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
    
    // More permissive policy for development
    options.AddPolicy("DevelopmentPolicy", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// ==================== Build App ====================
var app = builder.Build();

// ==================== Configure Middleware ====================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // Use more permissive CORS in development
    app.UseCors("DevelopmentPolicy");
}
else
{
    // Use restricted CORS in production
    app.UseCors("AllowWebAndMobile");
}

app.UseAuthorization();
app.MapControllers();

// ==================== Configure PORT for Render / Production ====================
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://*:{port}");

// ==================== Run App ====================
app.Run();
