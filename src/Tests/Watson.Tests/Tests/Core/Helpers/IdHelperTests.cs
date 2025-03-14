using Shouldly;
using Watson.Core.Helpers;

namespace Watson.Tests.Tests.Core.Helpers;

public class IdHelperTests
{
    #region Members

    private readonly IdHelper _sut = new();

    #endregion

    #region Tests

    [Test]
    public void GenerateId_ShouldReturnRandomIdOfLength()
    {
        // Arrange

        // Act
        var id = _sut.GenerateId();

        // Assert
        id.ShouldNotBeEmpty();
        id.Length.ShouldBe(8);
    }

    #endregion
}