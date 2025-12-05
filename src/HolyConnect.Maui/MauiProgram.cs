using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using HolyConnect.Application.Interfaces;
using HolyConnect.Application.Services;
using HolyConnect.Infrastructure.Persistence;
using HolyConnect.Infrastructure.Services;
using HolyConnect.Domain.Entities;
using System.Text.Json;

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

		// Helper to synchronously read storage path without async blocking
		string GetStoragePathSafe()
		{
			try
		{
			var appDataPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "HolyConnect");
			Directory.CreateDirectory(appDataPath);
			var settingsFile = Path.Combine(appDataPath, "settings.json");
			if (File.Exists(settingsFile))
			{
				var json = File.ReadAllText(settingsFile);
				var settings = JsonSerializer.Deserialize<AppSettings>(json);
				if (!string.IsNullOrWhiteSpace(settings?.StoragePath))
				{
					return settings!.StoragePath!;
				}
			}
			// Fallback to default path
			return appDataPath;
		}
		catch
		{
			return Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "HolyConnect");
		}
	}

		// Add repositories with file-based persistence without async blocking
		builder.Services.AddSingleton<IRepository<Domain.Entities.Environment>>(sp =>
		{
			return new FileBasedRepository<Domain.Entities.Environment>(
				e => e.Id,
				GetStoragePathSafe,
				"environments.json");
		});
		
		builder.Services.AddSingleton<IRepository<Collection>>(sp =>
		{
			return new FileBasedRepository<Collection>(
				c => c.Id,
				GetStoragePathSafe,
				"collections.json");
		});
		
		builder.Services.AddSingleton<IRepository<Request>>(sp =>
		{
			return new FileBasedRepository<Request>(
				r => r.Id,
				GetStoragePathSafe,
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
