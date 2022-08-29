// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using CommunityToolkit.Mvvm.SourceGenerators.ComponentModel.Models;
using CommunityToolkit.Mvvm.SourceGenerators.Extensions;
using CommunityToolkit.Mvvm.SourceGenerators.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CommunityToolkit.Mvvm.SourceGenerators;

/// <summary>
/// A source generator for the <c>ObservablePropertyAttribute</c> type.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed partial class ObservablePropertyGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Gather info for all annotated fields
        IncrementalValuesProvider<(HierarchyInfo Hierarchy, Result<PropertyInfo?> Info)> propertyInfoWithErrors =
            context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "CommunityToolkit.Mvvm.ComponentModel.ObservablePropertyAttribute",
                static (node, _) => node is VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Parent: FieldDeclarationSyntax { Parent: ClassDeclarationSyntax or RecordDeclarationSyntax, AttributeLists.Count: > 0 } } },
                static (context, token) =>
                {
                    if (!context.SemanticModel.Compilation.HasLanguageVersionAtLeastEqualTo(LanguageVersion.CSharp8))
                    {
                        return default;
                    }

                    IFieldSymbol fieldSymbol = (IFieldSymbol)context.TargetSymbol;

                    // Produce the incremental models
                    HierarchyInfo hierarchy = HierarchyInfo.From(fieldSymbol.ContainingType);
                    PropertyInfo? propertyInfo = Execute.TryGetInfo(fieldSymbol, out ImmutableArray<Diagnostic> diagnostics);

                    return (Hierarchy: hierarchy, new Result<PropertyInfo?>(propertyInfo, diagnostics));
                })
            .Where(static item => item.Hierarchy is not null);

        // Output the diagnostics
        context.ReportDiagnostics(propertyInfoWithErrors.Select(static (item, _) => item.Info.Errors));

        // Get the filtered sequence to enable caching
        IncrementalValuesProvider<(HierarchyInfo Hierarchy, PropertyInfo Info)> propertyInfo =
            propertyInfoWithErrors
            .Select(static (item, _) => (item.Hierarchy, Info: item.Info.Value))
            .Where(static item => item.Info is not null)!
            .WithComparers(HierarchyInfo.Comparer.Default, PropertyInfo.Comparer.Default);

        // Split and group by containing type
        IncrementalValuesProvider<(HierarchyInfo Hierarchy, ImmutableArray<PropertyInfo> Properties)> groupedPropertyInfo =
            propertyInfo
            .GroupBy(HierarchyInfo.Comparer.Default)
            .WithComparers(HierarchyInfo.Comparer.Default, PropertyInfo.Comparer.Default.ForImmutableArray());

        // Generate the requested properties and methods
        context.RegisterSourceOutput(groupedPropertyInfo, static (context, item) =>
        {
            // Generate all member declarations for the current type
            ImmutableArray<MemberDeclarationSyntax> memberDeclarations =
                item.Properties
                .Select(Execute.GetPropertySyntax)
                .Concat(item.Properties.Select(Execute.GetOnPropertyChangeMethodsSyntax).SelectMany(static l => l))
                .ToImmutableArray();

            // Insert all members into the same partial type declaration
            CompilationUnitSyntax compilationUnit = item.Hierarchy.GetCompilationUnit(memberDeclarations);

            context.AddSource($"{item.Hierarchy.FilenameHint}.g.cs", compilationUnit.GetText(Encoding.UTF8));
        });
        
        // Get all property declarations with at least one attribute
        IncrementalValuesProvider<IPropertySymbol> propertySymbols =
            context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is PropertyDeclarationSyntax { Parent: ClassDeclarationSyntax or RecordDeclarationSyntax, AttributeLists.Count: > 0 },
                static (context, _) => context.SemanticModel.GetDeclaredSymbol((PropertyDeclarationSyntax)context.Node)!
            );
        
        // Filter the fields using [DependsOn]
        IncrementalValuesProvider<IPropertySymbol> propertySymbolsWithAttribute =
            propertySymbols
                .Where(static item => item.HasAttributeWithFullyQualifiedName("global::CommunityToolkit.Mvvm.ComponentModel.DependsOnAttribute"));

        // Get set of all generated observable properties  
        IncrementalValueProvider<ImmutableDictionary<HierarchyInfo, ImmutableHashSet<string>>> observablePropertySet = propertyInfo
            .Collect()
            .Select(static (props, _) => props
                .GroupBy(
                    keySelector: static x => x.Hierarchy,
                    comparer: HierarchyInfo.Comparer.Default,
                    elementSelector: static x => x.Info.PropertyName
                )
                .ToImmutableDictionary(
                    keySelector: static x => x.Key,
                    keyComparer: HierarchyInfo.Comparer.Default,
                    elementSelector: ImmutableHashSet.ToImmutableHashSet
                )
            );


        // Gather info for all [DependsOn] properties
        IncrementalValuesProvider<(HierarchyInfo Hierarchy, Result<DependsOnPropertyInfo?> Info)> dependsOnPropertyWithErrors =
            propertySymbolsWithAttribute.Combine(observablePropertySet)
            .Select(static (combined, _) =>
            {
                (
                    IPropertySymbol propertySymbol, 
                    ImmutableDictionary<HierarchyInfo, ImmutableHashSet<string>> observableProperties
                ) = combined;
                HierarchyInfo hierarchy = HierarchyInfo.From(propertySymbol.ContainingType);

                (
                    DependsOnPropertyInfo? propertyInfo, 
                    ImmutableArray<Diagnostic> diagnostics
                ) = Execute.TryGetDependsOnInfo(
                    propertySymbol, 
                    observableProperties.GetValueOrDefault(hierarchy, ImmutableHashSet<string>.Empty)
                );
    
                return (hierarchy, new Result<DependsOnPropertyInfo?>(propertyInfo, diagnostics));
            });
        
        // Output the diagnostics
        context.ReportDiagnostics(dependsOnPropertyWithErrors.Select(static (item, _) => item.Info.Errors));
        
        // Get the filtered sequence to enable caching
        IncrementalValuesProvider<(HierarchyInfo Hierarchy, DependsOnPropertyInfo Info)> dependsOnPropertyInfo =
            dependsOnPropertyWithErrors
            .Select(static (item, _) => (item.Hierarchy, Info: item.Info.Value))
            .Where(static item => item.Info is not null)!
            .WithComparers(HierarchyInfo.Comparer.Default, DependsOnPropertyInfo.Comparer.Default);

        
        // Group by containing type and [ObservableProperty]
        IncrementalValuesProvider<(HierarchyInfo Hierarchy, InvertedDependsOnPropertyInfo Propertiey)> groupedDependsOnPropertyInfo =
            dependsOnPropertyInfo
            .Collect()
            .SelectMany(static (values, _) =>
                values
                    .SelectMany(static p => p.Info.ObservableProperties.Select(o => (
                        p.Hierarchy,
                        ObservableProperty: o,
                        DependentProperty: p.Info.PropertyName
                    )))
                    .GroupBy<
                        (HierarchyInfo Hierarchy, string ObservableProperty, string DependentProperty),
                        (HierarchyInfo Hierarchy, string ObservableProperty),
                        string,
                        (HierarchyInfo, InvertedDependsOnPropertyInfo)
                    >(
                        static x => (x.Hierarchy, x.ObservableProperty),
                        static x => x.DependentProperty,
                        static (o, p) => (
                            o.Hierarchy,
                            new InvertedDependsOnPropertyInfo(
                                o.ObservableProperty,
                                p.ToImmutableArray()
                            )
                        ),
                        new TupleComparer<HierarchyInfo, string>(
                            HierarchyInfo.Comparer.Default,
                            StringComparer.Ordinal
                        )
                    )
            )
            .WithComparers(HierarchyInfo.Comparer.Default, InvertedDependsOnPropertyInfo.Comparer.Default);

        
        // Generate the requested methods
        context.RegisterSourceOutput(groupedDependsOnPropertyInfo, static (context, item) =>
        {
            MemberDeclarationSyntax memberDeclarations = Execute.GetNotifyDependsOnMethodSyntax(item.Propertiey);
            
            CompilationUnitSyntax compilationUnit = item.Hierarchy.GetCompilationUnit(
                ImmutableArray.Create(memberDeclarations)
            );

            context.AddSource($"{item.Hierarchy.FilenameHint}_{item.Propertiey.ObservablePropertyName}.g.cs", compilationUnit.GetText(Encoding.UTF8));
        });
        
        // Gather all property changing names
        IncrementalValueProvider<ImmutableArray<string>> propertyChangingNames =
            propertyInfo
            .SelectMany(static (item, _) => item.Info.PropertyChangingNames)
            .Collect()
            .Select(static (item, _) => item.Distinct().ToImmutableArray())
            .WithComparer(EqualityComparer<string>.Default.ForImmutableArray());

        // Generate the cached property changing names
        context.RegisterSourceOutput(propertyChangingNames, static (context, item) =>
        {
            CompilationUnitSyntax? compilationUnit = Execute.GetKnownPropertyChangingArgsSyntax(item);

            if (compilationUnit is not null)
            {
                context.AddSource("__KnownINotifyPropertyChangingArgs.g.cs", compilationUnit.GetText(Encoding.UTF8));
            }
        });

        // Gather all property changed names
        IncrementalValueProvider<ImmutableArray<string>> propertyChangedNames =
            propertyInfo
            .SelectMany(static (item, _) => item.Info.PropertyChangedNames)
            .Collect()
            .Select(static (item, _) => item.Distinct().ToImmutableArray())
            .WithComparer(EqualityComparer<string>.Default.ForImmutableArray());

        // Generate the cached property changed names
        context.RegisterSourceOutput(propertyChangedNames, static (context, item) =>
        {
            CompilationUnitSyntax? compilationUnit = Execute.GetKnownPropertyChangedArgsSyntax(item);

            if (compilationUnit is not null)
            {
                context.AddSource("__KnownINotifyPropertyChangedArgs.g.cs", compilationUnit.GetText(Encoding.UTF8));
            }
        });

        // Get all class declarations with at least one attribute
        IncrementalValuesProvider<INamedTypeSymbol> classSymbols =
            context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                static (context, _) => (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(context.Node)!);

        // Filter only the type symbols with [NotifyPropertyChangedRecipients] and create diagnostics for them
        IncrementalValuesProvider<Diagnostic> notifyRecipientsErrors =
            classSymbols
            .Where(static item => item.HasAttributeWithFullyQualifiedName("global::CommunityToolkit.Mvvm.ComponentModel.NotifyPropertyChangedRecipientsAttribute"))
            .Select(static (item, _) => Execute.GetIsNotifyingRecipientsDiagnosticForType(item))
            .Where(static item => item is not null)!;

        // Output the diagnostics for [NotifyPropertyChangedRecipients]
        context.ReportDiagnostics(notifyRecipientsErrors);

        // Filter only the type symbols with [NotifyDataErrorInfo] and create diagnostics for them
        IncrementalValuesProvider<Diagnostic> notifyDataErrorInfoErrors =
            classSymbols
            .Where(static item => item.HasAttributeWithFullyQualifiedName("global::CommunityToolkit.Mvvm.ComponentModel.NotifyDataErrorInfoAttribute"))
            .Select(static (item, _) => Execute.GetIsNotifyDataErrorInfoDiagnosticForType(item))
            .Where(static item => item is not null)!;

        // Output the diagnostics for [NotifyDataErrorInfo]
        context.ReportDiagnostics(notifyDataErrorInfoErrors);
    }
}
