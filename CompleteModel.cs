namespace FlatDataGenerator;
internal record CompleteModel(
    string AssemblyName,
    ImmutableArray<ResultsModel> Results
);