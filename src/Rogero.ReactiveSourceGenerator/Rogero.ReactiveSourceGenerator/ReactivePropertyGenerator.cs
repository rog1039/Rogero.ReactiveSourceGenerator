using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Rogero.ReactiveSourceGenerator;

[Generator]
public class ReactivePropertyGenerator : IIncrementalGenerator
{
    private const string MakePropertyReactiveAttributeName = "MakeReactivePropertyAttribute";

    private static bool ShouldDebug = false;
    private static bool AddDebugFakeClass = false;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        try
        {
            context.RegisterPostInitializationOutput(ctx =>
            {
                // Add the marker attribute.
                //ctx.AddSource(
                //    "MakeReactivePropertyAttribute.g.cs",
                //    SourceText.From(MakeReactivePropertyAttributeSource.SourceText, Encoding.UTF8));
            });

            //Filter stage 1
            IncrementalValuesProvider<FieldDeclarationSyntax> fieldDeclarations = context
                .SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (node,          _) => IsInterestingFieldDeclarationSyntax(node),
                    transform: (syntaxContext, _) => GetFieldDeclsWithOurAttribute(syntaxContext))
                .Where(z => z is not null);

            IncrementalValueProvider<(Compilation, ImmutableArray<FieldDeclarationSyntax>)> compilationAndFields
                = context.CompilationProvider.Combine(fieldDeclarations.Collect());

            context.RegisterSourceOutput(compilationAndFields,
                (productionContext, tuple) => Execute(tuple.Item1, tuple.Item2, productionContext));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private static bool IsInterestingFieldDeclarationSyntax(SyntaxNode node)
    {
        return node is FieldDeclarationSyntax fieldDecl && fieldDecl.AttributeLists.Count > 0;
    }

    private static FieldDeclarationSyntax GetFieldDeclsWithOurAttribute(GeneratorSyntaxContext syntaxContext)
    {
        try
        {
            if (!Debugger.IsAttached && ShouldDebug)
                Debugger.Launch();
            var fieldDeclarationSyntax = (FieldDeclarationSyntax) syntaxContext.Node;

            // loop through all the attributes on the method
            foreach (AttributeListSyntax attributeListSyntax in fieldDeclarationSyntax.AttributeLists)
            {
                foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
                {
                    if (syntaxContext.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                    {
                        // weird, we couldn't get the symbol, ignore it
                        continue;
                    }

                    INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                    string           attributeName                      = attributeContainingTypeSymbol.Name;

                    if (attributeName == MakePropertyReactiveAttributeName)
                    {
                        return fieldDeclarationSyntax;
                    }
                }
            }

            // we didn't find the attribute we were looking for
            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return null;
    }

    private void Execute(Compilation compilation, ImmutableArray<FieldDeclarationSyntax> fields, SourceProductionContext context)
    {
        try
        {
            if(AddDebugFakeClass) context.AddSource("mytest.g.cs", $"public class TestingAbc{{ /* {DateTime.Now} */ }}");
        
            if (!Debugger.IsAttached && ShouldDebug)
                Debugger.Launch();
        
            if (fields.IsDefaultOrEmpty) return;

            IEnumerable<FieldDeclarationSyntax> distinctFields = fields.Distinct();

            var cancelToken = context.CancellationToken;
            List<PropertyGenerationInfo> propertyGenerationInfos =CreatePropertyGenerationInfo(compilation, fields, cancelToken);

            var propertiesByClass = propertyGenerationInfos
                .Distinct()
                .GroupBy(z => z.FullClassName)
                .ToList();

            var sb      = new StringBuilder();
            int counter = 1;
            foreach (var classProperties in propertiesByClass)
            {
                var result     = SourceCodeHelper.GetSourceCode(sb, classProperties.ToList());
                var className  = classProperties.Key;
                var sourceText = SourceText.From(result, Encoding.UTF8);
            
                if(!string.IsNullOrWhiteSpace(result))
                    context.AddSource("Gen_" + counter + ".g.cs", sourceText);
                counter++;
                sb.Clear();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private List<PropertyGenerationInfo> CreatePropertyGenerationInfo(Compilation                            compilation,
                                                                 ImmutableArray<FieldDeclarationSyntax> fields,
                                                                 CancellationToken                      cancellationToken)
    {
        try
        {
            var results = new List<PropertyGenerationInfo>();

            foreach (var fieldDeclSyntax in fields)
            {
                var semanticModel = compilation.GetSemanticModel(fieldDeclSyntax.SyntaxTree);
                foreach (var variableDeclaratorSyntax in fieldDeclSyntax.Declaration.Variables)
                {
                    if (cancellationToken.IsCancellationRequested) return results;
                
                    IFieldSymbol fieldSymbol = semanticModel.GetDeclaredSymbol(variableDeclaratorSyntax) as IFieldSymbol;

                    if (fieldSymbol is null) continue;

                    var propertyToGenerate = new PropertyGenerationInfo(fieldSymbol);
                    results.Add(propertyToGenerate);
                }
            }

            return results;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new List<PropertyGenerationInfo>();
        }
    }
}