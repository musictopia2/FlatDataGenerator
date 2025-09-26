namespace FlatDataGenerator;
internal class EmitClass(CompleteModel complete, SourceProductionContext context)
{
    public void Emit()
    {
        foreach (var item in complete.Results)
        {
            WriteItem(item);
        }
        WriteGlobal();
    }
    private void WriteGlobal()
    {
        SourceCodeStringBuilder builder = new();
        builder.WriteGlobalClass(complete.AssemblyName, w =>
        {
            foreach (var item in complete.Results)
            {
                w.WriteLine($"global::CommonBasicLibraries.AdvancedGeneralFunctionsAndProcesses.FlatDataHelpers.FlatDataHelpers<global::{item.Namespace}.{item.ClassName}>.MasterContext = new Flat{item.ClassName}Generator();");
            }
        });
        context.AddSource("FlatGlobalRegistratrions.g.cs", builder.ToString());
    }
    private void WriteItem(ResultsModel item)
    {
        SourceCodeStringBuilder builder = new();


        builder.WriteFlatClass(complete.AssemblyName, w =>
        {
            PopulateDetails(w, item);
        }, item);
        context.AddSource($"Flat{item.ClassName}.Generator.g.cs", builder.ToString()); //change sample to what you want.
    }
    private void PopulateDetails(ICodeBlock w, ResultsModel result)
    {
        w.AppendList(result.Properties)
            .AppendColumnCount(result)
            .AppendGetSingleHeader(result)
            .AppendGetHeaderList(result)
            .AppendGetValueByName(result)
            .AppendGetValueByIndex(result)
            .AppendSetValueByName(result)
            .AppendSetValueByIndex(result)
            ;
    }
}