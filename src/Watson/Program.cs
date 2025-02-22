using Microsoft.Extensions.DependencyInjection;
using Watson.Abstractions;
using Watson.Extensions;

var services = new ServiceCollection();
return await services.AddAppResources()
    .BuildServiceProvider()
    .GetService<ICli>()!
    .Run(args);