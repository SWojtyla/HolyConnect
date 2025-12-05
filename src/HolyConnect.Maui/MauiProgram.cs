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
			// Use lazy evaluation to avoid blocking during startup
			string GetStoragePath() => settingsService.GetSettingsAsync().GetAwaiter().GetResult().StoragePath;
			return new FileBasedRepository<Domain.Entities.Environment>(
				e => e.Id,
				GetStoragePath,
				"environments.json");
		});
		
		builder.Services.AddSingleton<IRepository<Collection>>(sp =>
		{
			var settingsService = sp.GetRequiredService<ISettingsService>();
			// Use lazy evaluation to avoid blocking during startup
			string GetStoragePath() => settingsService.GetSettingsAsync().GetAwaiter().GetResult().StoragePath;
			return new FileBasedRepository<Collection>(
				c => c.Id,
				GetStoragePath,
				"collections.json");
		});
		
		builder.Services.AddSingleton<IRepository<Request>>(sp =>
		{
			var settingsService = sp.GetRequiredService<ISettingsService>();
			// Use lazy evaluation to avoid blocking during startup
			string GetStoragePath() => settingsService.GetSettingsAsync().GetAwaiter().GetResult().StoragePath;
			return new FileBasedRepository<Request>(
				r => r.Id,
				GetStoragePath,
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
