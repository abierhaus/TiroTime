using TiroTime.Application.Common;

namespace TiroTime.Application.Tests.Common;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Empty(result.Error);
    }

    [Fact]
    public void Failure_ShouldCreateFailureResult()
    {
        // Act
        var result = Result.Failure("Something went wrong");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Something went wrong", result.Error);
    }

    [Fact]
    public void SuccessGeneric_ShouldCreateSuccessResultWithValue()
    {
        // Act
        var result = Result.Success(42);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
        Assert.Empty(result.Error);
    }

    [Fact]
    public void FailureGeneric_ShouldCreateFailureResultWithError()
    {
        // Act
        var result = Result.Failure<int>("Error occurred");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Error occurred", result.Error);
    }

    [Fact]
    public void SuccessGeneric_WithString_ShouldReturnValue()
    {
        // Act
        var result = Result.Success("test value");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("test value", result.Value);
    }

    [Fact]
    public void SuccessGeneric_WithComplexType_ShouldReturnValue()
    {
        // Arrange
        var testObject = new TestDto { Id = 1, Name = "Test" };

        // Act
        var result = Result.Success(testObject);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(testObject, result.Value);
        Assert.Equal(1, result.Value.Id);
        Assert.Equal("Test", result.Value.Name);
    }

    [Fact]
    public void Success_WithError_ShouldThrowInvalidOperationException()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            new TestResult(true, "error"));
    }

    [Fact]
    public void Failure_WithoutError_ShouldThrowInvalidOperationException()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            new TestResult(false, ""));
    }

    private class TestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class TestResult : Result
    {
        public TestResult(bool isSuccess, string error) : base(isSuccess, error)
        {
        }
    }
}
