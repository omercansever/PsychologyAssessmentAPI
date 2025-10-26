using Microsoft.EntityFrameworkCore;
using PsychologyAssessmentAPI.Data;
using PsychologyAssessmentAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Entity Framework yapılandırması
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// AutoMapper yapılandırması
builder.Services.AddAutoMapper(typeof(Program));

// Services
builder.Services.AddScoped<PsychologyAssessmentAPI.Services.IAssessmentService, PsychologyAssessmentAPI.Services.AssessmentService>();
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

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5128); // Tüm IP’lerden gelen istekleri dinle
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    // Development ortamında HTTPS yönlendirmesini devre dışı bırak
    // Bu mobil uygulama bağlantısı için önemli
    Console.WriteLine("Development mode: HTTPS redirection disabled for mobile app");
}
else
{
    // Production ortamında HTTPS kullan
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
        // Alternatif olarak migration kullanmak isterseniz:
        // context.Database.Migrate();
    }
    catch (Exception ex)
    {
        // Log the error
        Console.WriteLine($"Database oluşturulurken hata: {ex.Message}");
    }
}

Console.WriteLine($"API starting on: {string.Join(", ", builder.Configuration.GetSection("Kestrel:Endpoints").GetChildren().Select(x => x.Key))}");
Console.WriteLine("Mobile app can connect using:");
Console.WriteLine("- Emulator: http://10.0.2.2:5128/api");
Console.WriteLine("- Real device: http://192.168.1.82:5128/api");

app.Run();