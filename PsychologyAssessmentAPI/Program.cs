using Microsoft.EntityFrameworkCore;
using PsychologyAssessmentAPI.Data;
using PsychologyAssessmentAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Entity Framework yapılandırması (PostgreSQL - Render için)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// AutoMapper yapılandırması
builder.Services.AddAutoMapper(typeof(Program));

// Services
builder.Services.AddScoped<IAssessmentService, AssessmentService>();
builder.Services.AddScoped<IPsychologistService, PsychologistService>();

// CORS yapılandırması (frontend için) - Mobil uygulama için genişletilmiş
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

// Render ortamına uygun dinleme portu (Render PORT değişkenini otomatik kullanır)
builder.WebHost.ConfigureKestrel(options =>
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5128";
    options.ListenAnyIP(int.Parse(port));
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    Console.WriteLine("Development mode: HTTPS redirection disabled for mobile app");
}
else
{
    // Render Production ortamında HTTPS yönlendirmesi aktif kalabilir
    app.UseHttpsRedirection();
}

// CORS'u routing'den önce ekle
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Health check endpoint ekle
app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Database migration ve seed işlemi
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        context.Database.EnsureCreated();
        Console.WriteLine("Database created successfully");
        // İstersen migration kullanabilirsin:
        // context.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database oluşturulurken hata: {ex.Message}");
    }
}

Console.WriteLine("API running and ready for connections...");
app.Run();
