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
		builder.Services.AddSingleton<SettingsService>();

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

		// Add repositories with multi-file persistence for better performance with large collections
		builder.Services.AddSingleton<IRepository<Domain.Entities.Environment>>(sp =>
		{
			return new MultiFileRepository<Domain.Entities.Environment>(
				e => e.Id,
				GetStoragePathSafe,
				"environments",
				e => e.Name);
		});
		
		builder.Services.AddSingleton<IRepository<Collection>>(sp =>
		{
			return new MultiFileRepository<Collection>(
				c => c.Id,
				GetStoragePathSafe,
				"collections",
				c => c.Name);
		});
		
		builder.Services.AddSingleton<IRepository<Request>>(sp =>
		{
			return new MultiFileRepository<Request>(
				r => r.Id,
				GetStoragePathSafe,
				"requests",
				r => r.Name);
		});
		
		builder.Services.AddSingleton<IRepository<RequestHistoryEntry>>(sp =>
		{
			return new MultiFileRepository<RequestHistoryEntry>(
				h => h.Id,
				GetStoragePathSafe,
				"history",
				h => h.RequestName);
		});
		
		builder.Services.AddSingleton<IRepository<Flow>>(sp =>
		{
			return new MultiFileRepository<Flow>(
				f => f.Id,
				GetStoragePathSafe,
				"flows",
				f => f.Name);
		});

		// Add services
		builder.Services.AddScoped<IEnvironmentService, EnvironmentService>();
		builder.Services.AddScoped<ICollectionService, CollectionService>();
		builder.Services.AddScoped<IRequestService, RequestService>();
		builder.Services.AddScoped<IFlowService, FlowService>();
		builder.Services.AddScoped<IFormatterService, FormatterService>();
		builder.Services.AddScoped<IVariableResolver, VariableResolver>();
		builder.Services.AddScoped<IRequestHistoryService, RequestHistoryService>();
		builder.Services.AddScoped<IGitService>(sp => new GitService(GetStoragePathSafe));
		builder.Services.AddScoped<IResponseValueExtractor, ResponseValueExtractor>();
		builder.Services.AddScoped<IClipboardService, ClipboardService>();
		builder.Services.AddScoped<IGraphQLSchemaService, GraphQLSchemaService>();

		// Add request executors
		builder.Services.AddScoped<IRequestExecutor, RestRequestExecutor>();
		builder.Services.AddScoped<IRequestExecutor, GraphQLRequestExecutor>();
		builder.Services.AddScoped<IRequestExecutor, WebSocketRequestExecutor>();
		builder.Services.AddScoped<IRequestExecutor, GraphQLSubscriptionWebSocketExecutor>();
		builder.Services.AddScoped<IRequestExecutor, GraphQLSubscriptionSSEExecutor>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
