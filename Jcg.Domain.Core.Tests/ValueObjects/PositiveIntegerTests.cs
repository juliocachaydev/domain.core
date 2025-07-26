using Jcg.Domain.Core.Exceptions;
using Jcg.Domain.Core.ValueObjects;

namespace Jcg.Domain.Core.Tests.ValueObjects;

public class PositiveIntegerTests
{
    [Fact]
    public void Constructor_WhenValueIsGreaterThanZero_Creates()
    {
        // ***** ARRANGE *****

        var validValues = Enumerable.Range(1, 1000).ToArray();

        // for clarity, valid values for a positive integer are greater than zero
        Assert.True(validValues.All(v => v >= 1));

        foreach (var validValue in validValues)
        {
            // ***** ACT *****

            var result = new PositiveInteger(validValue);

            // ***** ASSERT *****

            Assert.Equal(validValue, result.Value);
        }
    }

    [Fact]
    public void Constructor_WhenValueLesserThanOrEqualToZero_Throws()
    {
        // ***** ARRANGE *****

        // not all possible but sufficient
        var invalidValues = Enumerable.Range(-1000, 1001).ToArray();
        // For Clarity, invalid values include zero
        Assert.True(invalidValues.All(v => v <= 0));


        foreach (var invalidValue in invalidValues)
        {
            // ***** ACT *****

            var result = Record.Exception(() => new PositiveInteger(invalidValue));

            // ***** ASSERT *****

            Assert.NotNull(result);
            Assert.IsType<InvalidEntityStateException>(result);
        }
    }

    [Fact]
    public void CanCreateRandomInSpecifiedRange()
    {
        // ***** ARRANGE *****

        // ***** ACT *****

        var result = Enumerable.Range(0, 10_000)
            .Select(_ => PositiveInteger.Random(10, 1000))
            .ToArray();

        // ***** ASSERT *****


        Assert.True(result.All(v => v.Value >= 10 && v.Value <= 1000));

        // Ensure they are different values, some duplicates are expected, but never all the same.
        Assert.NotEqual(result.Length, result.Distinct().Count());
    }

    [Fact]
    public void ToStringReturnsValueAsString()
    {
        // ***** ARRANGE *****

        var sut = new PositiveInteger(1);

        // ***** ACT *****

        var result = sut.ToString();

        // ***** ASSERT *****

        Assert.Equal("1", result);
    }
}