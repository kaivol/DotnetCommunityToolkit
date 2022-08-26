// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// This file is ported and adapted from ComputeSharp (Sergio0694/ComputeSharp),
// more info in ThirdPartyNotices.txt in the root of the project.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace CommunityToolkit.Mvvm.SourceGenerators.Diagnostics;

/// <summary>
/// Utility to build an <see cref="ImmutableArray{T}"/> of <see cref="Diagnostic"/>s 
/// </summary>
internal class DiagnosticsBuilder
{
    private readonly ImmutableArray<Diagnostic>.Builder diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

    /// <summary>
    /// Adds a new diagnostics.
    /// </summary>
    /// <param name="descriptor">The input <see cref="DiagnosticDescriptor"/> for the diagnostics to create.</param>
    /// <param name="symbol">The source <see cref="ISymbol"/> to attach the diagnostics to.</param>
    /// <param name="args">The optional arguments for the formatted message to include.</param>
    public void Add(
        DiagnosticDescriptor descriptor,
        ISymbol symbol,
        params object[] args
    )
    {
        this.diagnostics.Add(descriptor.CreateDiagnostic(symbol, args));
    }

    /// <summary>
    /// Build a <see cref="ImmutableArray{T}"/> of <see cref="Diagnostic"/>s from this builder.
    /// </summary>
    public ImmutableArray<Diagnostic> Build() => this.diagnostics.ToImmutable();
    
    /// <summary>
    /// Create an <see cref="ImmutableArray{T}"/> of <see cref="Diagnostic"/>s from the given diagnostic.
    /// </summary>
    /// <param name="descriptor">The input <see cref="DiagnosticDescriptor"/> for the diagnostics to create.</param>
    /// <param name="symbol">The source <see cref="ISymbol"/> to attach the diagnostics to.</param>
    /// <param name="args">The optional arguments for the formatted message to include.</param>
    public static ImmutableArray<Diagnostic> Create(
        DiagnosticDescriptor descriptor,
        ISymbol symbol,
        params object[] args
    )
    {
        DiagnosticsBuilder builder = new();
        builder.Add(descriptor, symbol, args);
        return builder.Build();
    }
}
