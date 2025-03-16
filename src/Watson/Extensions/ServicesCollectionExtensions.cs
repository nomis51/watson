using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Watson.Abstractions;
using Watson.Core;
using Watson.Core.Abstractions;
using Watson.Core.Helpers;
using Watson.Core.Helpers.Abstractions;
using Watson.Core.Repositories;
using Watson.Core.Repositories.Abstractions;
using Watson.Helpers;
using Watson.Helpers.Abstractions;
using Watson.Models;
using Watson.Models.Abstractions;

namespace Watson.Extensions;

public static class ServicesCollectionExtensions
{
    #region Public methods

    public static IServiceCollection AddAppResources(this IServiceCollection services)
    {
        ConfigureLogging(services);
        RegisterServices(services);

        services.AddScoped<IFileSystem, FileSystem>();
        services.AddSingleton<IConsoleAdapter, ConsoleAdapter>();
        services.AddSingleton<ICli, Cli>();
        return services;
    }

    #endregion

    #region Private methods

    private static void ConfigureLogging(IServiceCollection services)
    {
        services.AddLogging();
        services.AddSerilog();
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IAppDbContext, AppDbContext>();

        services.AddSingleton<IIdHelper, IdHelper>();
        services.AddSingleton<ITimeHelper, TimeHelper>();
        services.AddSingleton<IFrameHelper, FrameHelper>();
        services.AddSingleton<IDependencyResolver, DependencyResolver>();

        services.AddSingleton<IFrameRepository, FrameRepository>();
        services.AddSingleton<IProjectRepository, ProjectRepository>();
        services.AddSingleton<ITagRepository, TagRepository>();
        services.AddSingleton<ISettingsRepository, SettingsRepository>();
        services.AddSingleton<ITodoRepository, TodoRepository>();
        services.AddSingleton<IAliasRepository, AliasRepository>();
    }

    #endregion
}