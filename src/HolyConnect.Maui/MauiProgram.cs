using HolyConnect.Application.Interfaces;
using HolyConnect.Application.Services;
using HolyConnect.Domain.Entities;
using HolyConnect.Infrastructure.Persistence;
using HolyConnect.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using MudBlazor.Extensions;
using MudBlazor.Services;
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
        builder.Services.AddMudExtensions();
        // Add HttpClient
        builder.Services.AddHttpClient();
        builder.Services.AddScoped<HttpClient>();

        // Add Settings service
        builder.Services.AddSingleton<ISettingsService, FileBasedSettingsService>();
        builder.Services.AddSingleton<SettingsService>();

        // Add Secret Variables Repository and Service
        builder.Services.AddSingleton<ISecretVariablesRepository>(sp =>
            new Infrastructure.Persistence.SecretVariablesRepository(GetStoragePathSafe));
        builder.Services.AddScoped<ISecretVariablesService, SecretVariablesService>();

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
                e => e.Name,
                null, // No hierarchical path for environments (top-level)
                null); // No parent for environments
        });

        builder.Services.AddSingleton<IRepository<Collection>>(sp =>
        {
            // Helper to build hierarchical path for collections
            async Task<string> GetCollectionPath(Collection collection)
            {
                if (!collection.ParentCollectionId.HasValue)
                {
                    return string.Empty; // Root collection
                }
                
                var path = new List<string>();
                var currentId = collection.ParentCollectionId;
                var allCollections = await sp.GetRequiredService<IRepository<Collection>>().GetAllAsync();
                
                while (currentId.HasValue)
                {
                    var parent = allCollections.FirstOrDefault(c => c.Id == currentId.Value);
                    if (parent == null) break;
                    
                    path.Insert(0, SanitizeFolderName(parent.Name));
                    currentId = parent.ParentCollectionId;
                }
                
                return path.Count > 0 ? string.Join(Path.DirectorySeparatorChar, path) : string.Empty;
            }
            
            return new MultiFileRepository<Collection>(
                c => c.Id,
                GetStoragePathSafe,
                "collections",
                c => c.Name,
                GetCollectionPath,
                c => c.ParentCollectionId);
        });

        builder.Services.AddSingleton<IRepository<Request>>(sp =>
        {
            // Helper to build hierarchical path for requests
            async Task<string> GetRequestPath(Request request)
            {
                if (!request.CollectionId.HasValue)
                {
                    return string.Empty; // Request not in any collection
                }
                
                var path = new List<string>();
                var currentId = request.CollectionId;
                var allCollections = await sp.GetRequiredService<IRepository<Collection>>().GetAllAsync();
                
                while (currentId.HasValue)
                {
                    var collection = allCollections.FirstOrDefault(c => c.Id == currentId.Value);
                    if (collection == null) break;
                    
                    path.Insert(0, SanitizeFolderName(collection.Name));
                    currentId = collection.ParentCollectionId;
                }
                
                return path.Count > 0 ? string.Join(Path.DirectorySeparatorChar, path) : string.Empty;
            }
            
            return new MultiFileRepository<Request>(
                r => r.Id,
                GetStoragePathSafe,
                "requests",
                r => r.Name,
                GetRequestPath,
                r => r.CollectionId);
        });

        // Helper method to sanitize folder names
        static string SanitizeFolderName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            return name.Trim();
        }

        builder.Services.AddSingleton<IRepository<RequestHistoryEntry>>(sp =>
        {
            return new MultiFileRepository<RequestHistoryEntry>(
                h => h.Id,
                GetStoragePathSafe,
                "history");
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
        builder.Services.AddScoped<IDataGeneratorService, DataGeneratorService>();
        builder.Services.AddScoped<IVariableResolver>(sp => 
            new VariableResolver(sp.GetRequiredService<IDataGeneratorService>()));
        builder.Services.AddScoped<IRequestHistoryService, RequestHistoryService>();
        builder.Services.AddScoped<IGitService>(sp => new GitService(GetStoragePathSafe));
        builder.Services.AddScoped<IResponseValueExtractor, ResponseValueExtractor>();
        builder.Services.AddScoped<IClipboardService, ClipboardService>();
        builder.Services.AddScoped<IGraphQLSchemaService, GraphQLSchemaService>();
        builder.Services.AddScoped<IImportService, ImportService>();

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
