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

		// Add repositories
		builder.Services.AddSingleton<IRepository<Domain.Entities.Environment>>(
			new InMemoryRepository<Domain.Entities.Environment>(e => e.Id));
		builder.Services.AddSingleton<IRepository<Collection>>(
			new InMemoryRepository<Collection>(c => c.Id));
		builder.Services.AddSingleton<IRepository<Request>>(
			new InMemoryRepository<Request>(r => r.Id));

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
