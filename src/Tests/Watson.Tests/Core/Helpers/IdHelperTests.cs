using Shouldly;
using Watson.Core.Helpers;

namespace Watson.Tests.Core.Helpers;

public class IdHelperTests
{
    #region Members

    private readonly IdHelper _sut = new();

    #endregion

    #region Tests

    [InlineData(8)]
    [InlineData(10)]
    [InlineData(20)]
    [Theory]
    public void GenerateId_ShouldReturnRandomIdOfLength(int length)
    {
        // Arrange

        // Act
        var id = _sut.GenerateId(length);

        // Assert
        id.ShouldNotBeEmpty();
        id.Length.ShouldBe(length);
    }

    [Fact]
    public void GenerateId_ShouldThrow_WhenLengthIsLessThan8()
    {
        // Arrange
        // Act
        // Assert
        Should.Throw<ArgumentException>(() => _sut.GenerateId(7));
    }

    #endregion
}