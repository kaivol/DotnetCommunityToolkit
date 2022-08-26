// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CommunityToolkit.Mvvm.SourceGenerators.Extensions;
using CommunityToolkit.Mvvm.SourceGenerators.Helpers;

namespace CommunityToolkit.Mvvm.SourceGenerators.ComponentModel.Models;

/// <summary>
/// A model representing a [DependsOn] property
/// </summary>
/// <param name="PropertyName">The property name.</param>
/// <param name="ObservableProperties">The sequence of properties this property depends on.</param>
internal sealed record DependsOnPropertyInfo(
    string PropertyName,
    ImmutableArray<string> ObservableProperties)
{
    /// <summary>
    /// An <see cref="IEqualityComparer{T}"/> implementation for <see cref="DependsOnPropertyInfo"/>.
    /// </summary>
    public sealed class Comparer : Comparer<DependsOnPropertyInfo, Comparer>
    {
        /// <inheritdoc/>
        protected override void AddToHashCode(ref HashCode hashCode, DependsOnPropertyInfo obj)
        {
            hashCode.Add(obj.PropertyName);
            hashCode.AddRange(obj.ObservableProperties);
        }

        /// <inheritdoc/>
        protected override bool AreEqual(DependsOnPropertyInfo x, DependsOnPropertyInfo y)
        {
            return
                x.PropertyName == y.PropertyName &&
                x.ObservableProperties.SequenceEqual(y.ObservableProperties);
        }
    }
}

/// <summary>
/// A model representing the [DependsOn] properties of an observable property
/// </summary>
/// <param name="ObservablePropertyName">The observable property name.</param>
/// <param name="DependentProperties">The sequence of properties that depend on this property.</param>
internal sealed record InvertedDependsOnPropertyInfo(
    string ObservablePropertyName,
    ImmutableArray<string> DependentProperties)
{
    /// <summary>
    /// An <see cref="IEqualityComparer{T}"/> implementation for <see cref="DependsOnPropertyInfo"/>.
    /// </summary>
    public sealed class Comparer : Comparer<InvertedDependsOnPropertyInfo, Comparer>
    {
        /// <inheritdoc/>
        protected override void AddToHashCode(ref HashCode hashCode, InvertedDependsOnPropertyInfo obj)
        {
            hashCode.Add(obj.ObservablePropertyName);
            hashCode.AddRange(obj.DependentProperties);
        }

        /// <inheritdoc/>
        protected override bool AreEqual(InvertedDependsOnPropertyInfo x, InvertedDependsOnPropertyInfo y)
        {
            return
                x.ObservablePropertyName == y.ObservablePropertyName &&
                x.DependentProperties.SequenceEqual(y.DependentProperties);
        }
    }
}
