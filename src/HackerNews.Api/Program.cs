using HackerNews.Aplication.UseCases.GetStories;
using HackerNews.Infra.Repositories;
using HackerNews.Infra.Services;
using Refit;
using Serilog;
using Serilog.Events;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();

builder.Services.AddRefitClient<IHackerNewsApi>()
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri("https://hacker-news.firebaseio.com/");
        c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    })
    .AddPolicyHandler(GetRetryPolicy());

builder.Services.AddSingleton<Serilog.ILogger>(Log.Logger);

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IStoryRepository, HackerNewsStoryRepository>();
builder.Services.AddScoped<CachedStoryRepository>();
builder.Services.Decorate<IStoryRepository, CachedStoryRepository>();
builder.Services.AddScoped<IGetStoriesUseCase, GetStoriesUseCase>();

var app = builder.Build();

app.MapControllers();

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}