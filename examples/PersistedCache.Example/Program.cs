using PersistedCache;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddMySqlPersistedCache(builder.Configuration.GetConnectionString("MySql")!, options =>
{
    options.TableName = "persisted_cache";
});

builder.Services.AddPostgreSqlPersistedCache(builder.Configuration.GetConnectionString("PostgreSql")!);
builder.Services.AddSqlServerPersistedCache(builder.Configuration.GetConnectionString("SqlServer")!);
builder.Services.AddSqlitePersistedCache("Data Source=test.db");
builder.Services.AddMongoDbPersistedCache(builder.Configuration.GetConnectionString("MongoDb")!, "persistedcachedb");

var cachePath = AppDomain.CurrentDomain.BaseDirectory + "/cache";
builder.Services.AddFileSystemPersistedCache(cachePath);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/get", (IPersistedCache cache) => cache.Get<WeatherForecast[]>("weather_forecast"))
    .WithName("Get")
    .WithOpenApi();

app.MapPost("/set", (IPersistedCache cache) =>
    {
        var forecast = GetWeatherForecast();
        cache.Set("weather_forecast", forecast, Expire.InMinutes(5));

        return forecast;
    })
    .WithName("Set")
    .WithOpenApi();

app.MapGet("/get-or-set", (IPersistedCache cache) =>
    {
        var forecast = cache.GetOrSet("weather_forecast", GetWeatherForecast, Expire.InMinutes(5));
        return forecast;
    })
    .WithName("GetOrSet")
    .WithOpenApi();

app.MapDelete("/pull", (IPersistedCache cache) =>
    {
        var forecast = cache.Pull<WeatherForecast[]>("weather_forecast");
        return forecast;
    })
    .WithName("Pull")
    .WithOpenApi();

app.MapDelete("/forget", (IPersistedCache<FileSystemDriver> cache) => { cache.Forget("weather_forecast"); })
    .WithName("Forget")
    .WithOpenApi();

app.MapDelete("/flush", (IPersistedCache cache) => { cache.Flush(); })
    .WithName("Flush")
    .WithOpenApi();

app.MapDelete("flush/{pattern}", (IPersistedCache<MySqlDriver> cache, string pattern) => { cache.Flush(pattern); })
    .WithName("FlushPattern")
    .WithOpenApi();

app.MapDelete("/purge", (IPersistedCache cache) => { cache.Purge(); })
    .WithName("PurgeCache")
    .WithOpenApi();

// Async variants
app.MapGet("/get-async", async (IPersistedCache cache) => await cache.GetAsync<WeatherForecast[]>("weather_forecast"))
    .WithName("GetAsync")
    .WithOpenApi();

app.MapPost("/set-async", async (IPersistedCache cache) =>
    {
        var forecast = GetWeatherForecast();
        await cache.SetForeverAsync("weather_forecast", forecast);

        return forecast;
    })
    .WithName("SetAsync")
    .WithOpenApi();

app.MapGet("/get-or-set-async", async (IPersistedCache<SqlServerDriver> cache) =>
        await cache.GetOrSetAsync("weather_forecast", () => Task.FromResult(GetWeatherForecast), Expire.InMinutes(5)))
    .WithName("GetOrSetAsync")
    .WithOpenApi();

app.MapDelete("/pull-async", async (IPersistedCache cache) =>
        await cache.PullAsync<WeatherForecast[]>("weather_forecast"))
    .WithName("PullAsync")
    .WithOpenApi();

app.MapDelete("/forget-async", async (IPersistedCache cache) =>
        await cache.ForgetAsync("weather_forecast"))
    .WithName("ForgetAsync")
    .WithOpenApi();

app.MapDelete("/flush-async", async (IPersistedCache cache) =>
        await cache.FlushAsync())
    .WithName("FlushAsync")
    .WithOpenApi();

app.MapDelete("flush-async/{pattern}", async (IPersistedCache cache, string pattern) =>
        await cache.FlushAsync(pattern))
    .WithName("FlushPatternAsync")
    .WithOpenApi();

app.Run();


WeatherForecast[] GetWeatherForecast()
{
    return Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            ))
        .ToArray();
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}