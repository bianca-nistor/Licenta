using JobCv.Api.Data;
using JobCv.Api.Services;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddControllers();
builder.Services.AddHttpClient<AdzunaJobSearchService>();
builder.Services.AddScoped<MockAiService>();

builder.Services.AddHttpClient<GeminiAiService>((serviceProvider, httpClient) =>
{
    httpClient.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
    httpClient.Timeout = TimeSpan.FromSeconds(60);
});

builder.Services.AddHttpClient<OllamaAiService>((serviceProvider, httpClient) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

    var baseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";

    httpClient.BaseAddress = new Uri(baseUrl);
    httpClient.Timeout = TimeSpan.FromSeconds(120);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ICvTextExtractionService, CvTextExtractionService>();
builder.Services.AddScoped<ICvImportParserService, CvImportParserService>();


//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlite("Data Source=jobcv.db"));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        }));

builder.Services.AddScoped<PasswordService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();