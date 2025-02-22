using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Watson.Abstractions;
using Watson.Core;
using Watson.Core.Abstractions;
using Watson.Core.Helpers;
using Watson.Core.Helpers.Abstractions;

namespace Watson.Extensions;

public static class ServicesCollectionExtensions
{
   #region Public methods

   public static IServiceCollection AddAppResources(this IServiceCollection services)
   {
      ConfigureLogging(services);
      RegisterServices(services);
      
      services.AddSingleton<ICli, Cli>();
      return services;
   }

   #endregion

   #region Private methods
   
   private  static void ConfigureLogging(IServiceCollection services)
   {
      Log.Logger = new LoggerConfiguration()
         .WriteTo.File(
            Path.Join(
               Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
               $".{nameof(Watson).ToLower()}",
               "logs",
               ".txt"
            )
         )
         .CreateLogger();

      services.AddLogging();
      services.AddSerilog();
   }

   private static void RegisterServices(IServiceCollection services)
   {
      services.AddSingleton<IAppDbContext, AppDbContext>();
      services.AddSingleton<IIdHelper, IdHelper>();
   }

   #endregion
}