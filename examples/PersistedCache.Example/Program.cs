using PersistedCache;
using PersistedCache.MySql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMySqlPersistedCache(builder.Configuration.GetConnectionString("MySql")!, options =>
{
    options.TableName = "persisted_cache";
});

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

app.MapGet("/weatherforecast", (IPersistedCache cache) => cache.Get<int>("weather_forecast"))
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.MapPost("/weatherforecast", (IPersistedCache cache) =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();

        cache.SetForever("weather_forecast", forecast);
        
        return forecast;
    })
    .WithName("PostWeatherForecast")
    .WithOpenApi();

app.MapDelete("/weatherforecast", (IPersistedCache cache) =>
    {
        cache.Forget("weather_forecast");
    })
    .WithName("DeleteWeatherForecast")
    .WithOpenApi();

app.MapDelete("/flush", (IPersistedCache cache) =>
    {
        cache.Flush();
    })
    .WithName("FlushCache")
    .WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}