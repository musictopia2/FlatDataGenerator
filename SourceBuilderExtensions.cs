namespace FlatDataGenerator;
internal static class SourceBuilderExtensions
{
    public static void WriteGlobalClass(this SourceCodeStringBuilder builder, string ns, Action<ICodeBlock> action)
    {
        builder.WriteLine("#nullable enable")
                .WriteLine(w =>
                {
                    w.Write("namespace ")
                    .Write($"{ns}.FlatDataHelpers")
                    .Write(";");
                })
                .WriteLine(w =>
                {
                    w.Write($"public static class FlatRegistrationClasses");
                })
                .WriteCodeBlock(w =>
                {
                    w.WriteLine("public static void RegisterFlatDataClasses()")
                    .WriteCodeBlock(x =>
                    {
                        action.Invoke(x);
                    });
                });
    }
    public static void WriteFlatClass(this SourceCodeStringBuilder builder, string ns, Action<ICodeBlock> action, ResultsModel result)
    {
        builder.WriteLine("#nullable enable")
                .WriteLine(w =>
                {
                    w.Write("namespace ")
                    .Write($"{ns}.FlatDataHelpers")
                    .Write(";");
                })
                .WriteLine(w =>
                {
                    w.Write($"internal class Flat{result.ClassName}Generator")
                    .Write($": global::CommonBasicLibraries.AdvancedGeneralFunctionsAndProcesses.FlatDataHelpers.IFlatDataProvider<global::{result.Namespace}.{result.ClassName}>");
                })
                .WriteCodeBlock(action.Invoke);
    }
}