// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Linq;

namespace CommunityToolkit.Mvvm.ComponentModel;

/// <summary>
/// An attribute that can be used to support <see cref="ObservablePropertyAttribute"/> in generated properties. When this attribute is
/// used, the generated property setters of the properties specified in the attribute data will also call
/// <see cref="ObservableObject.OnPropertyChanged(string?)"/> (or the equivalent method in the target class) for the annotated property
/// specified in the attribute data. 
/// <para>
/// This attribute can be used as follows:
/// <code>
/// partial class MyViewModel : ObservableObject
/// {
///     [ObservableProperty]
///     private string name;
///
///     [ObservableProperty]
///     private string surname;
///
///     [DependsOn(nameof(Name), nameof(Surname))]
///     public string FullName => $"{Name} {Surname}";
/// }
/// </code>
/// </para>
/// And with this, code analogous to this will be generated:
/// <code>
/// partial class MyViewModel
/// {
///     public string Name
///     {
///         get => name;
///         set
///         {
///             if (SetProperty(ref name, value))
///             {
///                 OnPropertyChanged(nameof(FullName));
///             }
///         }
///     }
///
///     public string Surname
///     {
///         get => surname;
///         set
///         {
///             if (SetProperty(ref surname, value))
///             {
///                 OnPropertyChanged(nameof(FullName));
///             }
///         }
///     }
/// }
/// </code>
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
public sealed class DependsOnAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DependsOnAttribute"/> class.
    /// </summary>
    /// <param name="propertyName">The name of the observable property the annotated property depends on.</param>
    public DependsOnAttribute(string propertyName)
    {
        PropertyNames = new[] { propertyName };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DependsOnAttribute"/> class.
    /// </summary>
    /// <param name="propertyName">The name of the observable property the annotated property depends on.</param>
    /// <param name="otherPropertyNames">
    /// The other property names the annotated property depends on. This parameter can optionally
    /// be used to indicate a series of dependencies from the same attribute, to keep the code more compact.
    /// </param>
    public DependsOnAttribute(string propertyName, params string[] otherPropertyNames)
    {
        PropertyNames = new[] { propertyName }.Concat(otherPropertyNames).ToArray();
    }

    /// <summary>
    /// Gets the property names on which the annotated property depends.
    /// </summary>
    public string[] PropertyNames { get; }
}
