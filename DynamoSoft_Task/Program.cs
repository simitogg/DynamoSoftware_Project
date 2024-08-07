//using DynamoSoft_Task;
//using DynamoSoft_Task.Services;

//var builder = WebApplication.CreateBuilder(args);

//var startup = new Startup(builder.Configuration);

//startup.ConfigureServices(builder.Services);
////Add services to the container.

//builder.Services.AddControllers();

//var app = builder.Build();

//// Use a scoped service for seeding data
//using (var scope = app.Services.CreateScope())
//{
//    var dataSeeder = scope.ServiceProvider.GetRequiredService<CryptoService>();
//    await dataSeeder.SeedCoinsAsync();
//}
////Configure the HTTP request pipeline.
//startup.Configure(app, app.Environment);

//app.UseAuthorization();

//app.MapControllers();

//app.Run();
using DynamoSoft_Task.Context;
using DynamoSoft_Task.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// Configure services
builder.Services.AddDbContext<PortfolioDataContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("PortfolioContext")));

builder.Services.AddScoped<ICryptoService, CryptoService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<CryptoService>();
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<QueuedHostedService>();
builder.Services.AddHttpClient(); // Registers IHttpClientFactory which manages HttpClient instances
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        builder =>
        {
            builder.WithOrigins("http://localhost:3000")
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
        });
});
builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddLogging();

var app = builder.Build();

// Ensure the database is created and migrated
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PortfolioDataContext>();
    context.Database.EnsureCreated();

    // Seed data asynchronously
    var dataSeeder = scope.ServiceProvider.GetRequiredService<CryptoService>();
    Task.Run(async () => await dataSeeder.SeedCoinsAsync()).GetAwaiter().GetResult();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseRouting();
app.UseCors("AllowSpecificOrigins");
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<PortfolioHub>("/portfolioHub");
    endpoints.MapControllers();
});

app.Run();

