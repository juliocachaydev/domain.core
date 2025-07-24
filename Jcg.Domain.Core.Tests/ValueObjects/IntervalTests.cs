using System.ComponentModel.Design.Serialization;
using Jcg.Domain.Core.Exceptions;
using Jcg.Domain.Core.ValueObjects;

namespace Jcg.Domain.Core.Tests.ValueObjects;

public class IntervalTests
{
    [Theory]
    [InlineData(10,15, false)]
    [InlineData(10,10, false)]
    [InlineData(10,9, true)]
    public void Constructor_AssertsEndIsSameOrMoreThanStart(
        int start, int end, bool shouldThrow)
    {
        // ***** ARRANGE *****

        // ***** ACT *****
        
        var result = Record.Exception(() => new Interval<int>(start, end));

        // ***** ASSERT *****

        if (shouldThrow)
        {
            Assert.IsType<InvalidEntityStateException>(result);
        }
        else
        {
            Assert.Null(result);
        }
    }

    [Theory]
    // Only the ends of the interval overlap, but this is inclusive so that is an overlap
    [InlineData(10,20,20,30,true)]
    [InlineData(10,20,0,10, true)]
    // One interval contained in another
    [InlineData(0,10,1,9,true)]
    // Both intervals are the same
    [InlineData(0,10,0,10,true)]
    // Some overlap
    [InlineData(0,10,9,20, true)]
    [InlineData(0,10,-10,1, true)]
    // no overlap
    [InlineData(0,10,11,20,false)]
    [InlineData(0,10,-10,-1, false)]
    public void OverlapsInclusiveWith_ChecksIfOverlapsWithAnotherInterval_IncludingStartAndEnd(
        int s1, int e1, int s2, int e2, bool shouldOverlap)
    {
        // ***** ARRANGE *****

        var i1 = new Interval<int>(s1, e1);
        var i2 = new Interval<int>(s2, e2);

        // ***** ACT *****

        var result = i1.OverlapsInclusiveWith(i2);

        // ***** ASSERT *****
        
        Assert.Equal(shouldOverlap, result);
    }

    [Theory]
    // Only the ends of the interval overlap, but this is exclusive so that is not an overlap
    [InlineData(10,20,20,30,false)]
    [InlineData(10,20,0,10, false)]
    // One interval contained in another
    [InlineData(0,10,1,9,true)]
    // Both intervals are the same
    [InlineData(0,10,0,10,true)]
    // Some overlap
    [InlineData(0,10,9,20, true)]
    [InlineData(0,10,-10,1, true)]
    // no overlap
    [InlineData(0,10,11,20,false)]
    [InlineData(0,10,-10,-1, false)]
    public void OverlapsExclusiveWith_ChecksIfOverlapsWithAnotherInterval_ExcludingStartAndEnd(
        int s1, int e1, int s2, int e2, bool shouldOverlap)
    {
        // ***** ARRANGE *****

        var i1 = new Interval<int>(s1, e1);
        var i2 = new Interval<int>(s2, e2);

        // ***** ACT *****

        var result = i1.OverlapsExclusiveWith(i2);

        // ***** ASSERT *****
        
        Assert.Equal(shouldOverlap, result);
    }

    [Fact]
    public void CanMapIntervalToDifferenType()
    {
        // ***** ARRANGE *****
        
        var i1 = new Interval<int>(100, 200);

        // ***** ACT *****

        var i2 = i1.MapTo(x => x.ToString());

        // ***** ASSERT *****

        Assert.IsType<Interval<String>>(i2);
        Assert.Equal("100", i2.Start);
        Assert.Equal("200", i2.End);
    }
}