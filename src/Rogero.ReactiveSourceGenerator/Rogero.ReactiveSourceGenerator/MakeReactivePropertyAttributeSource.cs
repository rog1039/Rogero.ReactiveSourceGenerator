using System.Data.SqlTypes;

namespace Rogero.ReactiveSourceGenerator;

public static class MakeReactivePropertyAttributeSource
{
    public const string SourceText = @"
namespace Rogero.ReactiveSourceGenerator.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class MakeReactivePropertyAttribute : Attribute { }
";
}