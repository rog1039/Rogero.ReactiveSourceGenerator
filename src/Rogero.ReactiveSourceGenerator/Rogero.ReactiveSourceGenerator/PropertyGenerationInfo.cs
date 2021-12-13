using Microsoft.CodeAnalysis;

namespace Rogero.ReactiveSourceGenerator;

/// <summary>
/// Contains the information necessary to generate the ReactiveUI property we are interested in.
/// </summary>
public readonly struct PropertyGenerationInfo
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
}