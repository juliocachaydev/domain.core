using System;
using Jcg.Domain.Core.Exceptions;

namespace Jcg.Domain.Core.ValueObjects;

/// <summary>
/// Models an interval with a start and end value.
/// </summary>
public record Interval<T> where T: IComparable<T>
{
    /// <summary>
    /// The Start Value of the Interval.
    /// </summary>
    public T Start { get; }

    /// <summary>
    /// The End value of the interval. Is always greater than or equal to Start.
    /// </summary>
    public T End { get; }
    
    /// <summary>
    /// Creates an Interval, asserting that the End value of the interval must be greater than or equal to the Start value.
    /// </summary>
    /// <param name="start">The start value of the interval.</param>
    /// <param name="end">The end value of the interval.</param>
    /// <exception cref="InvalidEntityStateException">
    /// Thrown when the end value is less than the start value.
    /// </exception>
    public Interval(T start, T end)
    {
        if (end.CompareTo(start) < 0)
        {
            throw new InvalidEntityStateException("End must be greater than or equal to start.");
        }

        Start = start;
        End = end;
    }

    /// <summary>
    /// Determines whether this interval overlaps inclusively with another interval.
    /// Inclusive overlap means the intervals share at least one value, including endpoints.
    /// </summary>
    /// <param name="other">The other interval to check for overlap.</param>
    /// <returns>
    /// True if the intervals overlap inclusively; otherwise, false.
    /// </returns>
    public bool OverlapsInclusiveWith(Interval<T> other)
    {
        // Overlap occurs if the start of one interval is less than or equal to the end of the other, and vice versa
        return Start.CompareTo(other.End) <= 0 && End.CompareTo(other.Start) >= 0;
    }
    
    /// <summary>
    /// Determines whether this interval overlaps exclusively with another interval.
    /// Exclusive overlap means the intervals share at least one value, but not at their endpoints.
    /// </summary>
    /// <param name="other">The other interval to check for exclusive overlap.</param>
    /// <returns>
    /// True if the intervals overlap exclusively; otherwise, false.
    /// </returns>
    public bool OverlapsExclusiveWith(Interval<T> other)
    {
        // Exclusive overlap: intervals share values, but not at endpoints
        return Start.CompareTo(other.End) < 0 && End.CompareTo(other.Start) > 0;
    }
    
    /// <summary>
    /// Maps the start and end values of this interval to a new type using the provided mapping function.
    /// </summary>
    /// <typeparam name="U">The type to map the interval values to. Must implement <see cref="IComparable{U}"/>.</typeparam>
    /// <param name="mapFunc">A function that maps a value of type <typeparamref name="T"/> to type <typeparamref name="U"/>.</param>
    /// <returns>
    /// A new <see cref="Interval{U}"/> with the mapped start and end values.
    /// </returns>
    public Interval<U> MapTo<U>(Func<T, U> mapFunc) where U : IComparable<U>
    {
        var newStart = mapFunc(Start);
        var newEnd = mapFunc(End);
        return new Interval<U>(newStart, newEnd);
    }
    
    
}