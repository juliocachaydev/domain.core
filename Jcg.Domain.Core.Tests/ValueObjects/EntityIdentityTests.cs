using Jcg.Domain.Core.Exceptions;
using Jcg.Domain.Core.ValueObjects;

namespace Jcg.Domain.Core.Tests.ValueObjects;

public class EntityIdentityTests
{
    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void AssertsValueNotEmpty(
        bool isEmpty, bool shouldThrow)
    {
        // ***** ARRANGE *****
        
        // ***** ACT *****

        var result = Record.Exception(() => new EntityIdentity(isEmpty ? Guid.Empty : Guid.NewGuid()));

        // ***** ASSERT *****

        if (shouldThrow)
        {
            Assert.IsType<InvalidEntityStateException>(result);
            Assert.Matches("empty", result?.Message);
        }
        else
        {
            Assert.Null(result);
        }
    }

    [Fact]
    public void ToStringReturnsValue()
    {
        // ***** ARRANGE *****

        var idValue = Guid.NewGuid();

        var obj = new EntityIdentity(idValue);

        // ***** ACT *****

        var result = obj.ToString();

        // ***** ASSERT *****
        
        Assert.Equal(idValue.ToString(), result);
    }

    [Fact]
    public void TryParse_WhenParsable_ReturnsTrue_ParsesObject()
    {
        // ***** ARRANGE *****

        var idValue = Guid.NewGuid().ToString();

        // ***** ACT & ASSERT *****
        
        if (EntityIdentity.TryParse(idValue, out var result))
        {
            Assert.NotNull(result);
            Assert.Equal(idValue.ToString(), result.Value.ToString());
        }
        else
        {
            Assert.Fail("This should never happen because the value is a parsable GUID.");
        }
        
    }
    
    [Fact]
    public void TryParse_WhenNotParsable_ReturnsFalse_ObjectIsNull()
    {
        // ***** ARRANGE *****

        var idValue = "Not a GUID";

        // ***** ACT & ASSERT *****
        
        if (EntityIdentity.TryParse(idValue, out var result))
        {
            Assert.Fail("This should never happen because the value is not parsable to GUID");
        }
        else
        {
            Assert.Null(result);
        }
        
    }
    
    
    [Fact]
    public void Parse_WhenParsable_ReturnsObject()
    {
        // ***** ARRANGE *****
        
        var idValue = Guid.NewGuid().ToString();

        // ***** ACT *****

        var result = EntityIdentity.Parse(idValue);

        // ***** ASSERT *****
        
        Assert.Equal(idValue, result.Value.ToString());
    }
    
    [Fact]
    public void Parse_WhenNotParsable_ThrowsException()
    {
        // ***** ARRANGE *****

        var idValue = "Not a GUID";

        // ***** ACT *****

        var result = Record.Exception(() => EntityIdentity.Parse(idValue));

        // ***** ASSERT *****

        Assert.IsType<InvalidEntityStateException>(result);
        Assert.Matches("cannot be parsed", result?.Message);
    }

    [Fact]
    public void CanCreateRandom()
    {
        // ***** ARRANGE *****

        // ***** ACT *****

        var values = Enumerable.Range(0, 100).Select(_ => EntityIdentity.Random).ToArray();

        // ***** ASSERT *****
        
       Assert.Equal(values.Length, values.Distinct().Count());
    }
}