using Microsoft.Extensions.DependencyInjection;
using Watson.Abstractions;
using Watson.Extensions;

var services = new ServiceCollection();
services.AddAppResources()
    .BuildServiceProvider()
    .GetService<ICli>()!
    .Run();