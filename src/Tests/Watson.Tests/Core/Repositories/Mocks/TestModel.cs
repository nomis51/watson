using System.ComponentModel;
using Watson.Core.Models.Database.Abstractions;

namespace Watson.Tests.Core.Repositories.Mocks;

[Description("Tests")]
public class TestModel : DbModel
{
    public string Name { get; set; } = string.Empty;
}