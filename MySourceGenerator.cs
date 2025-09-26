namespace FlatDataGenerator;
[Generator] //this is important so it knows this class is a generator which will generate code for a class using it.
public class MySourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Step 1: Get all class nodes that match the syntax (candidate classes)
        IncrementalValuesProvider<NodeInformation> declares1 = context.SyntaxProvider.CreateSyntaxProvider(
                 (s, _) => IsSyntaxTarget(s),
                 (t, _) => GetTarget(t))
            .Where(static m => m is not null)!;

        // Step 2: Collect node infos and combine with Compilation
        var declares2 = context.CompilationProvider.Combine(declares1.Collect());

        // Step 3: Parse and flatten results
        var declares3 = declares2.Select((tuple, _) =>
        {
            var (compilation, nodeList) = tuple;
            var results = new List<ResultsModel>();

            foreach (var node in nodeList)
            {
                results.AddRange(ExtractResultsModels(node, compilation));
            }

            return new CompleteModel(compilation.AssemblyName ?? "UnknownAssembly", [.. results]);
        });

        context.RegisterSourceOutput(declares3, Execute);
    }
    private ImmutableArray<ResultsModel> ExtractResultsModels(NodeInformation nodeInformation, Compilation compilation)
    {
        ParserClass parses = new(nodeInformation, compilation);
        BasicList<ResultsModel> output = parses.GetResults();
        return [.. output];
    }
    private bool IsSyntaxTarget(SyntaxNode syntax)
    {
        bool rets = syntax is ClassDeclarationSyntax;
        return rets;
    }
    private NodeInformation? GetTarget(GeneratorSyntaxContext context)
    {
        var ourClass = context.GetClassNode(); //can use the sematic model at this stage
        if (ourClass == null)
        {
            return null;
        }
        if (context.SemanticModel.GetDeclaredSymbol(context.Node) is not INamedTypeSymbol symbol)
        {
            return null;
        }
        // Check for fluent style inheritance
        if (symbol.InheritsFrom("BaseFlatDataSettingsContext") && symbol.Name != "BaseFlatDataSettingsContext")
        {
            return new NodeInformation
            {
                Node = ourClass,
                Source = EnumSourceCategory.Fluent
            };
        }
        // Check for marker interface
        if (symbol.Implements("IFlatMarker"))
        {
            return new NodeInformation
            {
                Node = ourClass,
                Source = EnumSourceCategory.MarkerInterface
            };
        }
        return null;
    }

    private void Execute(SourceProductionContext context, CompleteModel complete)
    {
        var grouped = complete.Results.GroupBy(x => x.ClassName);
        foreach (var group in grouped)
        {
            if (group.Count() > 1)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        id: "FDG_DUPLICATE",
                        title: "Duplicate Flat Data Class",
                        messageFormat: "The class '{0}' was configured more than once for flat data generation.",
                        category: "FlatDataGenerator",
                        DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    Location.None,
                    group.Key));
            }
        }
        // Stop code gen if duplicates exist
        if (grouped.Any(g => g.Count() > 1))
        {
            return;
        }
        EmitClass emit = new(complete, context);
        emit.Emit();
    }
}