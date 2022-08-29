using System;
using System.Collections.Generic;

namespace CommunityToolkit.Mvvm.SourceGenerators.Extensions;

/// <summary>
/// An <see cref="IEqualityComparer{T}"/> implementation for a value tuple.
/// </summary>
public sealed class TupleComparer<TLeft, TRight> : IEqualityComparer<(TLeft Left, TRight Right)>
{
    /// <summary>
    /// The <typeparamref name="TLeft"/> comparer.
    /// </summary>
    private readonly IEqualityComparer<TLeft> comparerLeft;

    /// <summary>
    /// The <typeparamref name="TRight"/> comparer.
    /// </summary>
    private readonly IEqualityComparer<TRight> comparerRight;

    /// <summary>
    /// Creates a new <see cref="TupleComparer{TLeft,TRight}"/> instance with the specified parameters.
    /// </summary>
    /// <param name="comparerLeft">The <typeparamref name="TLeft"/> comparer.</param>
    /// <param name="comparerRight">The <typeparamref name="TRight"/> comparer.</param>
    public TupleComparer(
        IEqualityComparer<TLeft> comparerLeft,
        IEqualityComparer<TRight> comparerRight
    )
    {
        this.comparerLeft = comparerLeft;
        this.comparerRight = comparerRight;
    }

    /// <inheritdoc/>
    public bool Equals((TLeft Left, TRight Right) x, (TLeft Left, TRight Right) y)
    {
        return
            this.comparerLeft.Equals(x.Left, y.Left) &&
            this.comparerRight.Equals(x.Right, y.Right);
    }

    /// <inheritdoc/>
    public int GetHashCode((TLeft Left, TRight Right) obj)
    {
        return HashCode.Combine(
            this.comparerLeft.GetHashCode(obj.Left),
            this.comparerRight.GetHashCode(obj.Right));
    }
}

/// <summary>
/// An <see cref="IEqualityComparer{T}"/> implementation for a value tuple.
/// </summary>
public sealed class TupleComparer<T1, T2, T3> : IEqualityComparer<(T1, T2, T3)>
{
    /// <summary>
    /// The <typeparamref name="T1"/> comparer.
    /// </summary>
    private readonly IEqualityComparer<T1> comparer1;

    /// <summary>
    /// The <typeparamref name="T2"/> comparer.
    /// </summary>
    private readonly IEqualityComparer<T2> comparer2;

    /// <summary>
    /// The <typeparamref name="T3"/> comparer.
    /// </summary>
    private readonly IEqualityComparer<T3> comparer3;

    /// <summary>
    /// Creates a new <see cref="TupleComparer{T1,T2,T3}"/> instance with the specified parameters.
    /// </summary>
    /// <param name="comparer1">The <typeparamref name="T1"/> comparer.</param>
    /// <param name="comparer2">The <typeparamref name="T2"/> comparer.</param>
    /// <param name="comparer3">The <typeparamref name="T3"/> comparer.</param>
    public TupleComparer(
        IEqualityComparer<T1> comparer1,
        IEqualityComparer<T2> comparer2,
        IEqualityComparer<T3> comparer3
    )
    {
        this.comparer1 = comparer1;
        this.comparer2 = comparer2;
        this.comparer3 = comparer3;
    }

    /// <inheritdoc/>
    public bool Equals((T1, T2, T3) x, (T1, T2, T3) y)
    {
        return
            this.comparer1.Equals(x.Item1, y.Item1) &&
            this.comparer2.Equals(x.Item2, y.Item2) &&
            this.comparer3.Equals(x.Item3, y.Item3);
    }

    /// <inheritdoc/>
    public int GetHashCode((T1, T2, T3) obj)
    {
        return HashCode.Combine(
            this.comparer1.GetHashCode(obj.Item1),
            this.comparer2.GetHashCode(obj.Item2),
            this.comparer3.GetHashCode(obj.Item3));
    }
}