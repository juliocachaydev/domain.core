using Jcg.Domain.Core.Exceptions;
using Jcg.Domain.Core.ValueObjects;

namespace Jcg.Domain.Core.Tests.ValueObjects;

public class NonEmptyStringTests
{
    [Theory]
    [InlineData("Hello", false)]
    [InlineData(" ", true)]
    [InlineData("    ", true)]
    public void AssertsValueNonEmptyNorWhitespace(
        string value, bool shouldThrow)
    {
        // ***** ARRANGE *****

        // ***** ACT *****

        var result = Record.Exception(() => new NonEmptyString(value));

        // ***** ASSERT *****

        if (shouldThrow)
        {
            Assert.NotNull(result);
            Assert.IsType<InvalidEntityStateException>(result);
        }
        else
        {
            Assert.Null(result);
        }
    }

    [Theory]
    [InlineData("Hello", "Hello")]
    [InlineData("    Hello World   ", "Hello World")]
    public void Constructor_RemovesWhiteSpace(
        string value, string expectedValue)
    {
        // ***** ARRANGE *****

        var sut = new NonEmptyString(value);

        // ***** ACT *****

        var result = sut.Value;

        // ***** ASSERT *****

        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public void CanCreateRandom()
    {
        // ***** ARRANGE *****

        // ***** ACT *****

        // Even when the strings are very long, there is a tiny chance of having a duplicate, so this
        // test may fail occasionally. The chance is so small that I decided to accept it.
        var result = Enumerable.Range(0, 100)
            .Select(_ => NonEmptyString.Random(200)).ToArray();

        // ***** ASSERT *****

        Assert.Equal(100, result.Distinct().Count());
    }

    [Fact]
    public void CanCreateRandomForSpecifiedLength()
    {
        for (var i = 1; i < 200; i++)
        {
            var result = NonEmptyString.Random(i);
            Assert.Equal(i, result.Value.Length);
        }
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // ***** ARRANGE *****

        var sut = new NonEmptyString("Hello World");

        // ***** ACT *****

        var result = sut.ToString();

        // ***** ASSERT *****

        Assert.Equal("Hello World", result);
    }
}