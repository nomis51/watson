using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Watson.Abstractions;

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
               Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
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
      
   }

   #endregion
}