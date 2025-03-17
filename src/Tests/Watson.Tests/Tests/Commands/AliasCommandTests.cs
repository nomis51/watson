using NSubstitute;
using Shouldly;
using Watson.Commands;
using Watson.Core.Helpers;
using Watson.Core.Helpers.Abstractions;
using Watson.Core.Models.Database;
using Watson.Core.Repositories;
using Watson.Core.Repositories.Abstractions;
using Watson.Helpers;
using Watson.Models;
using Watson.Models.CommandLine;
using Watson.Tests.Abstractions;
using Xunit;

namespace Watson.Tests.Tests.Commands;

public class AliasCommandTests : CommandWithConsoleTest
{
    #region Members

    private readonly IAliasRepository _aliasRepository = Substitute.For<IAliasRepository>();
    private readonly AliasCommand _sut;

    #endregion

    #region Constructors

    public AliasCommandTests()
    {
        var idHelper = new IdHelper();
        _aliasRepository.InsertAsync(Arg.Any<Alias>())
            .Returns(e =>
            {
                var alias = e.Arg<Alias>();
                alias.Id = idHelper.GenerateId();
                return alias;
            });
        _aliasRepository.DeleteAsync(Arg.Any<string>())
            .Returns(true);

        _sut = new AliasCommand(
            new DependencyResolver(
                new ProjectRepository(DbContext, idHelper),
                Substitute.For<IFrameRepository>(),
                new TagRepository(DbContext, idHelper),
                new TimeHelper(),
                new FrameHelper(Substitute.For<IFrameRepository>()),
                Substitute.For<ISettingsRepository>(),
                new TodoRepository(DbContext, idHelper),
                ConsoleAdapter,
                _aliasRepository,
                Substitute.For<IProcessHelper>()
            )
        );
    }

    #endregion

    #region Tests

    [Test]
    public async Task Run_ShouldCreateAlias()
    {
        // Arrange
        var options = new AliasOptions
        {
            Arguments = ["test", "status"]
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        await _aliasRepository.Received()
            .InsertAsync(Arg.Is<Alias>(a =>
                a.Name == "test" &&
                a.Command == "status"
            ));
    }

    [Arguments("create")]
    [Arguments("add")]
    [Arguments("set")]
    [Test]
    public async Task Run_ShouldCreateAlias_WithExplicitCreateAction(string action)
    {
        // Arrange
        var options = new AliasOptions
        {
            Arguments = [action, "test", "status"]
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        await _aliasRepository.Received()
            .InsertAsync(Arg.Is<Alias>(a =>
                a.Name == "test" &&
                a.Command == "status"
            ));
    }

    [Arguments("delete")]
    [Arguments("remove")]
    [Test]
    public async Task Run_ShouldDeleteAlias(string action)
    {
        // Arrange
        var options = new AliasOptions
        {
            Arguments = [action, "test"]
        };
        _aliasRepository.GetByNameAsync(Arg.Any<string>())
            .Returns(e => new Alias
            {
                Id = "id1",
                Name = e.Arg<string>(),
                Command = "test"
            });

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        await _aliasRepository.Received()
            .GetByNameAsync("test");
        await _aliasRepository.Received()
            .DeleteAsync("id1");
    }

    [Test]
    public async Task Run_ShouldFailToRemove_WhenAliasDoesNotExist()
    {
        // Arrange
        var options = new AliasOptions
        {
            Arguments = ["remove", "test"]
        };

        _aliasRepository.GetByNameAsync("test")
            .Returns(default(Alias));

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
        await _aliasRepository.DidNotReceive()
            .DeleteAsync(Arg.Any<string>());
        GetConsoleOutput().ShouldBe(
            GenerateSpectreMarkupOutput("[red]Alias not found.[/]")
        );
    }

    [Test]
    public async Task Run_ShouldFailToCreate_WhenAliasAlreadyExists()
    {
        // Arrange
        var options = new AliasOptions
        {
            Arguments = ["create", "test", "status"]
        };

        _aliasRepository.GetByNameAsync("test")
            .Returns(new Alias());

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
        await _aliasRepository.DidNotReceive()
            .InsertAsync(Arg.Any<Alias>());
        GetConsoleOutput().ShouldBe(
            GenerateSpectreMarkupOutput("[red]Alias already exists.[/]")
        );
    }

    [Test]
    public async Task Run_ShouldFailToCreate_WhenAliasIsReserved()
    {
        // Arrange
        var options = new AliasOptions
        {
            Arguments = ["create", "status", "status"]
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(1);
        await _aliasRepository.DidNotReceive()
            .InsertAsync(Arg.Any<Alias>());
    }

    [Test]
    public async Task Run_ShouldCreateAlias_WithOptions()
    {
        // Arrange
        var options = new AliasOptions
        {
            Arguments = ["breakfast", "start", "cooking", "bacon", "--at", "8"],
        };

        // Act
        var result = await _sut.Run(options);

        // Assert
        result.ShouldBe(0);
        await _aliasRepository.Received()
            .InsertAsync(Arg.Is<Alias>(e =>
                e.Name == "breakfast" &&
                e.Command == "start cooking bacon --at 8"
            ));
    }

    #endregion
}