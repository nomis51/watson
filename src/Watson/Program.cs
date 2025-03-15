using System.Diagnostics;
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
    if (Debugger.IsAttached) throw;
    Log.Fatal(e, "An unhandled exception occurred.");
}

return 1;