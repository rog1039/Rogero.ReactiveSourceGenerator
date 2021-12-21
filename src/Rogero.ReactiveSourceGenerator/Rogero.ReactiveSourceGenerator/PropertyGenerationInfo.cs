using Microsoft.CodeAnalysis;

namespace Rogero.ReactiveSourceGenerator;

/// <summary>
/// Contains the information necessary to generate the ReactiveUI property we are interested in.
/// </summary>
public readonly struct PropertyGenerationInfo : IEquatable<PropertyGenerationInfo>
{
    public readonly string Namespace;
    public readonly string ClassName;
    public readonly string FieldName;
    public readonly string FullClassName;
    public readonly string FullFieldTypeName;
    public readonly string PropertyName;

    public readonly IFieldSymbol _fieldSymbol;

    public PropertyGenerationInfo(IFieldSymbol fieldSymbol)
    {
        _fieldSymbol = fieldSymbol;
        var containingSymbol = fieldSymbol.ContainingSymbol;
        Namespace         = containingSymbol.ContainingNamespace.ToDisplayString();
        ClassName         = containingSymbol.Name;
        FieldName         = fieldSymbol.Name;
        FullClassName     = $"{Namespace}.{ClassName}";
        FullFieldTypeName = fieldSymbol.Type.Name;
        PropertyName      = GetFieldName(fieldSymbol.Name);
    }

    public static string GetFieldName(string fieldName)
    {
        for (int i = 0; i < fieldName.Length; i++)
        {
            var character = fieldName[i];
            if (character == '_') continue;
            if (char.IsLower(character))
            {
                var upper = char.ToUpper(character);
                return upper + fieldName.Substring(i + 1, fieldName.Length - i - 1);
            }

            throw new Exception("Invalid field name. Must start with _ and then lowercase char or just a lowercase char.");
        }

        throw new Exception("Invalid field name. Must start with _ and then lowercase char or just a lowercase char.");
    }

    private sealed class ClassNameFieldNameEqualityComparer : IEqualityComparer<PropertyGenerationInfo>
    {
        public bool Equals(PropertyGenerationInfo x, PropertyGenerationInfo y)
        {
            return x.ClassName == y.ClassName && x.FieldName == y.FieldName;
        }

        public int GetHashCode(PropertyGenerationInfo obj)
        {
            unchecked
            {
                return (obj.ClassName.GetHashCode() * 397) ^ obj.FieldName.GetHashCode();
            }
        }
    }

    public static IEqualityComparer<PropertyGenerationInfo> ClassNameFieldNameComparer { get; } = new ClassNameFieldNameEqualityComparer();

    public bool Equals(PropertyGenerationInfo other)
    {
        return ClassName == other.ClassName && FieldName == other.FieldName;
    }

    public override bool Equals(object? obj)
    {
        return obj is PropertyGenerationInfo other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (ClassName.GetHashCode() * 397) ^ FieldName.GetHashCode();
        }
    }

    public static bool operator ==(PropertyGenerationInfo left, PropertyGenerationInfo right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PropertyGenerationInfo left, PropertyGenerationInfo right)
    {
        return !left.Equals(right);
    }
}