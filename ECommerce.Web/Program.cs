using ECommerce.Bot.Models;
using ECommerce.Bot.Services;
using ECommerce.Data.DbContexts;
using ECommerce.Data.IRepositories;
using ECommerce.Data.Repositories;
using ECommerce.Web.Components;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<BotConfiguration>(
            builder.Configuration.GetSection(BotConfiguration.Configuration));

builder.Services.AddDbContext<ECommerceDbContext>(options => options
    .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.
    AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddHttpClient("telegram_bot_client")
    .AddTypedClient<ITelegramBotClient>((httpClient) =>
    {
        var botConfig = builder.Configuration
            .GetSection(BotConfiguration.Configuration)
            .Get<BotConfiguration>();
        TelegramBotClientOptions options = new(botConfig!.BotToken);
        return new TelegramBotClient(options, httpClient);
    });
builder.Services.AddScoped<UpdateHandler>();
builder.Services.AddScoped<ReceiverService>();
builder.Services.AddHostedService<PollingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
