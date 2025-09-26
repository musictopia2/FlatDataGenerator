namespace FlatDataGenerator;
internal record ResultsModel : ICustomResult
{
    public string ClassName { get; set; } = "";
    public string Namespace { get; set; } = "";
    public BasicList<PropertyModel> Properties { get; set; } = [];
}