using System.Text;

namespace Rogero.ReactiveSourceGenerator;

public static class SourceCodeHelper
{
    public static string GetSourceCode(StringBuilder sb, IList<PropertyGenerationInfo> fields)
    {
        sb.Append(@"
using ReactiveUI;
");

        sb.Append(@"
namespace ").Append(fields.First().Namespace).Append(";");

        sb.Append(@"
public partial class ").Append(fields.First().ClassName);
        
        sb.Append(@"
{");
        
        foreach (var property in fields)
        {
            sb.Append(@"
    public ").Append(property._fieldSymbol.Type).Append(" ").Append(property.PropertyName);
            sb.Append(@"
    {
        get => ").Append(property.FieldName).Append(";").Append(@"
        set => this.RaiseAndSetIfChanged(ref ").Append(property.FieldName).Append(@", value);
    }").AppendLine();
        }
        
        sb.Append(@"
}");

        return sb.ToString();
    }
}