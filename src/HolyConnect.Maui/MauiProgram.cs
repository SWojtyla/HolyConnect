using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using HolyConnect.Application.Interfaces;
using HolyConnect.Application.Services;
using HolyConnect.Infrastructure.Persistence;
using HolyConnect.Infrastructure.Services;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

		// Add MudBlazor services
		builder.Services.AddMudServices();

		// Add HttpClient
		builder.Services.AddHttpClient();
		builder.Services.AddScoped<HttpClient>();

		// Add Settings service
		builder.Services.AddSingleton<ISettingsService, FileBasedSettingsService>();
		builder.Services.AddScoped<SettingsService>();

		// Add repositories with file-based persistence
		builder.Services.AddSingleton<IRepository<Domain.Entities.Environment>>(sp =>
		{
			var settingsService = sp.GetRequiredService<ISettingsService>();
			return new FileBasedRepository<Domain.Entities.Environment>(
				e => e.Id,
				() => settingsService.GetSettingsAsync().GetAwaiter().GetResult().StoragePath,
				"environments.json");
		});
		
		builder.Services.AddSingleton<IRepository<Collection>>(sp =>
		{
			var settingsService = sp.GetRequiredService<ISettingsService>();
			return new FileBasedRepository<Collection>(
				c => c.Id,
				() => settingsService.GetSettingsAsync().GetAwaiter().GetResult().StoragePath,
				"collections.json");
		});
		
		builder.Services.AddSingleton<IRepository<Request>>(sp =>
		{
			var settingsService = sp.GetRequiredService<ISettingsService>();
			return new FileBasedRepository<Request>(
				r => r.Id,
				() => settingsService.GetSettingsAsync().GetAwaiter().GetResult().StoragePath,
				"requests.json");
		});

		// Add services
		builder.Services.AddScoped<EnvironmentService>();
		builder.Services.AddScoped<CollectionService>();
		builder.Services.AddScoped<RequestService>();

		// Add request executors
		builder.Services.AddScoped<IRequestExecutor, RestRequestExecutor>();
		builder.Services.AddScoped<IRequestExecutor, GraphQLRequestExecutor>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
