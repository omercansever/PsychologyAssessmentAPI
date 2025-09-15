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

// CORS yapılandırması (frontend için)
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

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Database migration ve seed işlemi
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    try
    {
        context.Database.EnsureCreated();
        // Alternatif olarak migration kullanmak isterseniz:
        // context.Database.Migrate();
    }
    catch (Exception ex)
    {
        // Log the error
        Console.WriteLine($"Database oluşturulurken hata: {ex.Message}");
    }
}

app.Run();