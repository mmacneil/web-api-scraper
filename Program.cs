using Hangfire;
using WebApiScraper;
using WebApiScraper.Infrastructure;
using WebApiScraper.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Hangfire services.
builder.Services.AddHangfire(configuration =>
    configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(builder.Configuration.GetConnectionString("AppDb"))
);

// Add the processing server as IHostedService
builder.Services.AddHangfireServer();

// configure DI for application services
builder.Services.AddSingleton<DatabaseContext>();
builder.Services.AddScoped<StatisticsCrawler>();
builder.Services.AddScoped<PlayerRepository>();
builder.Services.AddScoped<StatisticsRepository>();

builder.Services.AddHostedService<SchedulerTask>();

var app = builder.Build();

// ensure database and tables exist
using var scope = app.Services.CreateScope();
var databaseUtils = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
await databaseUtils.Init();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHangfireDashboard(
    "/hangfire",
    new DashboardOptions() { DashboardTitle = "Hangfire Dashboard" }
);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
