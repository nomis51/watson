using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Watson.Abstractions;
using Watson.Extensions;
using Watson.Helpers;

LoggingHelper.Configure();

try
{
    var services = new ServiceCollection();
    return await services.AddAppResources()
        .BuildServiceProvider()
        .GetService<ICli>()!
        .Run(args);
}
catch (Exception e)
{
    Log.Fatal(e, "An unhandled exception occurred.");
}

return 1;