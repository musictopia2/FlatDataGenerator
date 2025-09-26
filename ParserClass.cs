namespace FlatDataGenerator;
internal class ParserClass(NodeInformation node, Compilation compilation)
{
    public BasicList<ResultsModel> GetResults()
    {
        if (node.Source == EnumSourceCategory.MarkerInterface)
        {
            var temp = GetResult(node.Node!);
            return [temp];
        }
        return GetFluentResults();
    }
    private BasicList<ResultsModel> GetFluentResults()
    {
        ParseContext context = new(compilation, node.Node!);
        var members = node.Node!.DescendantNodes().OfType<MethodDeclarationSyntax>();
        foreach (var m in members)
        {
            var symbol = context.SemanticModel.GetDeclaredSymbol(m) as IMethodSymbol;
            if (symbol is not null && symbol.Name == "Configure") //has to be magic strings now.
            {
                BasicList<ResultsModel> output = [];
                ParseSettings(output, context, m);
                return output;
            }
        }
        return [];
    }
    private void ParseSettings(BasicList<ResultsModel> results, ParseContext context, MethodDeclarationSyntax syntax)
    {
        var makeCalls = ParseUtils.FindCallsOfMethodWithName(context, syntax, "Make");
        foreach (CallInfo make in makeCalls)
        {
            ResultsModel result = new();
            results.Add(result);
            INamedTypeSymbol makeType = (INamedTypeSymbol)make.MethodSymbol.TypeArguments[0]!;
            result.ClassName = makeType.Name;
            result.Namespace = makeType.ContainingNamespace.ToDisplayString();
            var properties = makeType.GetAllPublicProperties();
            foreach (var property in properties)
            {
                result.Properties.Add(property.GetStartingPropertyInformation<PropertyModel>());
            }
        }
    }
    private ResultsModel GetResult(ClassDeclarationSyntax classDeclaration)
    {
        ResultsModel output;
        INamedTypeSymbol symbol = compilation.GetClassSymbol(classDeclaration)!;
        output = symbol.GetStartingResults<ResultsModel>();
        var properties = symbol.GetAllPublicProperties();
        foreach (var property in properties)
        {
            output.Properties.Add(property.GetStartingPropertyInformation<PropertyModel>());
        }
        return output;
    }
}