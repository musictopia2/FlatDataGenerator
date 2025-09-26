namespace FlatDataGenerator;
internal static class WriterExtensions
{
    public static ICodeBlock AppendList(this ICodeBlock w, BasicList<PropertyModel> properties)
    {
        BasicList<string> names = properties.Select(items => items.PropertyName).ToBasicList();
        w.WriteLine("private readonly global::CommonBasicLibraries.CollectionClasses.BasicList<string> _properties = ")
            .WriteListBlock(names);
        return w;
    }
    public static ICodeBlock AppendColumnCount(this ICodeBlock w, ResultsModel result)
    {
        w.WriteLine($"int {GetInterfacePrefix(result)}ColumnCount => _properties.Count;");
        return w;
    }
    private static string GetInterfacePrefix(ResultsModel result)
    {
        return $"global::CommonBasicLibraries.AdvancedGeneralFunctionsAndProcesses.FlatDataHelpers.IFlatDataProvider<global::{result.Namespace}.{result.ClassName}>.";
    }
    public static ICodeBlock AppendGetSingleHeader(this ICodeBlock w, ResultsModel result)
    {
        w.WriteLine($"string {GetInterfacePrefix(result)}GetHeader(int index)")
            .WriteCodeBlock(w =>
            {
                w.WriteLine("return _properties[index];");
            });
        return w;
    }
    public static ICodeBlock AppendGetHeaderList(this ICodeBlock w, ResultsModel result)
    {
        w.WriteLine($"global::CommonBasicLibraries.CollectionClasses.BasicList<string> {GetInterfacePrefix(result)}GetHeaders()")
            .WriteCodeBlock(w =>
            {
                w.WriteLine("return global::CommonBasicLibraries.AdvancedGeneralFunctionsAndProcesses.BasicExtensions.Lists.ToBasicList(_properties);");
            });
        return w;
    }
    public static ICodeBlock AppendGetValueByName(this ICodeBlock w, ResultsModel result)
    {
        w.WriteLine($"string {GetInterfacePrefix(result)}GetValue(global::{result.Namespace}.{result.ClassName} item, string propertyName)")
             .WriteCodeBlock(w =>
             {
                 foreach (var item in result.Properties)
                 {
                     w.WriteLine($"""
                         if (propertyName == "{item.PropertyName}")
                         """)
                     .WriteCodeBlock(w =>
                     {
                         w.PrivateCompleteGetValue(item);
                            
                     });
                 }
                 w.WriteLine("""
                     throw new ArgumentException($"Invalid property name for {propertyName}", nameof(propertyName));
                     """);
             });
        return w;
    }
    private static ICodeBlock PrivateCompleteGetValue(this ICodeBlock w, PropertyModel item)
    {
        switch (item.VariableCustomCategory)
        {
            case EnumSimpleTypeCategory.String:
                if (item.Nullable == true)
                {
                    w.PrivateNullableStringGetValue(item.PropertyName);
                }
                else
                {
                    w.PrivateSimpleGetValue(item.PropertyName);
                }
                break;
            case EnumSimpleTypeCategory.Bool:
            case EnumSimpleTypeCategory.StandardEnum:
            case EnumSimpleTypeCategory.Char:
            case EnumSimpleTypeCategory.Decimal:
            case EnumSimpleTypeCategory.Double:
            case EnumSimpleTypeCategory.Int:
            case EnumSimpleTypeCategory.Float:
            case EnumSimpleTypeCategory.DateOnly:
            case EnumSimpleTypeCategory.DateTime:
            case EnumSimpleTypeCategory.DateTimeOffset:
            case EnumSimpleTypeCategory.TimeSpan:
            case EnumSimpleTypeCategory.TimeOnly:
                if (item.Nullable == true)
                {
                    w.PrivateNullableHasValueForGetValue(item.PropertyName);
                }
                else
                {
                    w.PrivateStringGetValue(item.PropertyName);
                }
                break;
            case EnumSimpleTypeCategory.CustomEnum:
                if (item.Nullable == true)
                {
                    w.PrivateNullableCustomEnumGetValue(item.PropertyName);
                }
                else
                {
                    w.PrivateCustomEnumGetValue(item.PropertyName);
                }
                break;
            default:
                if (item.Nullable == true)
                {
                    w.PrivateNullableStringGetValue(item.PropertyName);
                }
                else
                {
                    w.PrivateStringGetValue(item.PropertyName);
                }
                break;
        }
        return w;
    }
    public static ICodeBlock AppendGetValueByIndex(this ICodeBlock w, ResultsModel result)
    {
        w.WriteLine($"string {GetInterfacePrefix(result)}GetValue(global::{result.Namespace}.{result.ClassName} item, int index)")
             .WriteCodeBlock(w =>
             {
                 int x = 0;
                 foreach (var item in result.Properties)
                 {
                     w.WriteLine($"if (index == {x})")
                     .WriteCodeBlock(w =>
                     {
                         w.PrivateCompleteGetValue(item);
                     });
                     x++;
                 }
                 w.WriteLine("""
                        throw new ArgumentException($"Invalid index for {index}", nameof(index));
                        """);
             });
        return w;
    }
    private static ICodeBlock PrivateSimpleGetValue(this ICodeBlock w, string propertyName)
    {
        w.WriteLine($"return item.{propertyName};");
        return w;
    }
    private static ICodeBlock PrivateStringGetValue(this ICodeBlock w, string propertyName)
    {
        w.WriteLine($"return item.{propertyName}.ToString();");
        return w;
    }
    private static ICodeBlock PrivateCustomEnumGetValue(this ICodeBlock w, string propertyName)
    {
        w.WriteLine($"return item.{propertyName}.Name;");
        return w;
    }
    private static ICodeBlock PrivateNullableCustomEnumGetValue(this ICodeBlock w, string propertyName)
    {
        w.WriteLine($"""
            return item.{propertyName}.HasValue ? item.{propertyName}.Value.Name : "";
            """);
        return w;
    }
    private static ICodeBlock PrivateNullableHasValueForGetValue(this ICodeBlock w, string propertyName)
    {
        w.WriteLine($"""
            return item.{propertyName}.HasValue ? item.{propertyName}.Value.ToString() : "";
            """);
        return w;
    }
    private static ICodeBlock PrivateNullableStringGetValue(this ICodeBlock w, string propertyName)
    {
        w.WriteLine($"""
            return string.IsNullOrWhiteSpace(item.{propertyName}) ? "" : item.{propertyName};
            """);
        return w;
    }
    public static ICodeBlock AppendSetValueByName(this ICodeBlock w, ResultsModel result)
    {
        w.WriteLine($"bool {GetInterfacePrefix(result)}TrySetValue(ref global::{result.Namespace}.{result.ClassName} item, string property, string value, out string? errorMessage)")
             .WriteCodeBlock(w =>
             {
                foreach (var item in result.Properties)
                {
                    w.WriteLine($"""
                        if (property == "{item.PropertyName}")
                        """);
                    w.WriteCodeBlock(w =>
                    {
                        w.PrivateCompleteSetValue(item);
                    });
                 }
                 w.WriteLine("""
                     throw new ArgumentException($"Invalid property name for {property}", nameof(property));
                     """);
             });
        return w;
    }
    public static ICodeBlock AppendSetValueByIndex(this ICodeBlock w, ResultsModel result)
    {
        w.WriteLine($"bool {GetInterfacePrefix(result)}TrySetValue(ref global::{result.Namespace}.{result.ClassName} item, int index, string value, out string? errorMessage)")
             .WriteCodeBlock(w =>
             {
                 int x = 0;
                 foreach (var item in result.Properties)
                 {
                     w.WriteLine($"""
                         if (index == {x})
                         """);
                     w.WriteCodeBlock(w =>
                     {
                         w.PrivateCompleteSetValue(item);
                     });
                     x++;
                 }
                 w.WriteLine("""
                     throw new ArgumentException($"Invalid property index for {index}", nameof(index));
                     """);
             });
        return w;
    }
    private static ICodeBlock PrivateCompleteSetValue(this ICodeBlock w, PropertyModel item)
    {
        if (item.Nullable == false)
        {
            w.PrivateBasicSetValues(item);
            return w;            
        }
        w.PrivateNullableSetValues(item);
        return w;
    }
    private static ICodeBlock PrivateNullableSetValues(this ICodeBlock w, PropertyModel item)
    {
        w.WriteLine("if (string.IsNullOrWhiteSpace(value))")
            .WriteCodeBlock(w =>
            {
               w.WriteLine($"item.{item.PropertyName} = null;")
                    .ShowTrue();
            });
        w.PrivateBasicSetValues(item);
        return w;
    }
    private static ICodeBlock PrivateBasicSetValues(this ICodeBlock w, PropertyModel item)
    {
        if (item.VariableCustomCategory == EnumSimpleTypeCategory.String)
        {
            w.WriteLine($"item.{item.PropertyName} = value;");
            w.ShowTrue();
            return w;
        }
        if (item.VariableCustomCategory == EnumSimpleTypeCategory.Int)
        {
            string lower = item.PropertyName.ChangeCasingForVariable(EnumVariableCategory.ParameterCamelCase);
            w.WriteLine($"if (int.TryParse(value, out int {lower}))")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine($"item.{item.PropertyName} = {lower};");
                    w.ShowTrue();
                })
                .WriteLine("else")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine($$"""
                            errorMessage = $"Invalid integer value for {{item.PropertyName}}.  Trying to use {value}";
                            """)
                    .WriteLine("return false;");
                });
            return w;
        }
        if (item.VariableCustomCategory == EnumSimpleTypeCategory.Bool)
        {
            string lower = item.PropertyName.ChangeCasingForVariable(EnumVariableCategory.ParameterCamelCase);
            w.WriteLine($"if (bool.TryParse(value, out bool {lower}))")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine($"item.{item.PropertyName} = {lower};");
                    w.ShowTrue();
                })
                .WriteLine("else")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine($$"""
                            errorMessage = $"Invalid boolean value for {{item.PropertyName}}.  Trying to use {value}";
                            """)
                    .WriteLine("return false;");
                });
            ;
            return w;
        }
        if (item.VariableCustomCategory == EnumSimpleTypeCategory.StandardEnum)
        {
            string lower = item.PropertyName.ChangeCasingForVariable(EnumVariableCategory.ParameterCamelCase);
            w.WriteLine($"if (Enum.TryParse(value, out global::{item.ContainingNameSpace}.{item.UnderlyingSymbolName} {lower}))")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine($"item.{item.PropertyName} = ({item.UnderlyingSymbolName}){lower};");
                    w.ShowTrue();
                })
                .WriteLine("else")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine($$"""
                            errorMessage = $"Invalid enum value for {{item.PropertyName}}.  Trying to use {value}";
                            """)
                    .WriteLine("return false;");
                });
            return w;
        }
        if (item.VariableCustomCategory == EnumSimpleTypeCategory.CustomEnum)
        {
            w.WriteLine("try")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine($"item.{item.PropertyName} = global::{item.ContainingNameSpace}.{item.UnderlyingSymbolName}.FromName(value, true);")
                        .ShowTrue();
                })
                .WriteLine("catch (global::System.Exception)")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine($$"""
                            errorMessage = $"Invalid enum value for {{item.PropertyName}}.  Trying to use {value}";
                            """)
                    .WriteLine("return false;");
                });
            return w;
        }
        if (item.VariableCustomCategory == EnumSimpleTypeCategory.Char)
        {
            string lower = item.PropertyName.ChangeCasingForVariable(EnumVariableCategory.ParameterCamelCase);
            w.WriteLine($"if (char.TryParse(value, out char {lower}))")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine($"item.{item.PropertyName} = {lower};");
                    w.ShowTrue();
                })
                .WriteLine("else")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine($$"""
                            errorMessage = $"Invalid char value for {{item.PropertyName}}.  Trying to use {value}";
                            """)
                    .WriteLine("return false;");
                });
            return w;
        }
        if (item.VariableCustomCategory == EnumSimpleTypeCategory.Decimal)
        {
            string lower = item.PropertyName.ChangeCasingForVariable(EnumVariableCategory.ParameterCamelCase);
            w.WriteLine($"if (decimal.TryParse(value, out decimal {lower}))")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine($"item.{item.PropertyName} = {lower};");
                    w.ShowTrue();
                })
                .WriteLine("else")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine($$"""
                            errorMessage = $"Invalid decimal value for {{item.PropertyName}}.  Trying to use {value}";
                            """)
                    .WriteLine("return false;");
                });
            return w;
        }
        if (item.VariableCustomCategory == EnumSimpleTypeCategory.Double)
        {
            string lower = item.PropertyName.ChangeCasingForVariable(EnumVariableCategory.ParameterCamelCase);
            w.WriteLine($"if (double.TryParse(value, out double {lower}))")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine($"item.{item.PropertyName} = {lower};");
                    w.ShowTrue();
                })
                .WriteLine("else")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine($$"""
                            errorMessage = $"Invalid double value for {{item.PropertyName}}.  Trying to use {value}";
                            """)
                    .WriteLine("return false;");
                });
            return w;
        }
        if (item.VariableCustomCategory == EnumSimpleTypeCategory.Float)
        {
            string lower = item.PropertyName.ChangeCasingForVariable(EnumVariableCategory.ParameterCamelCase);
            w.WriteLine($"if (float.TryParse(value, out float {lower}))")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine($"item.{item.PropertyName} = {lower};");
                    w.ShowTrue();
                })
                .WriteLine("else")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine($$"""
                            errorMessage = $"Invalid float value for {{item.PropertyName}}.  Trying to use {value}";
                            """)
                    .WriteLine("return false;");
                });
            return w;
        }
        if (item.VariableCustomCategory == EnumSimpleTypeCategory.DateOnly)
        {
            string lower = item.PropertyName.ChangeCasingForVariable(EnumVariableCategory.ParameterCamelCase);
            w.WriteLine($"if (DateOnly.TryParse(value, out DateOnly {lower}))")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine($"item.{item.PropertyName} = {lower};");
                    w.ShowTrue();
                })
                .WriteLine("else")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine($$"""
                            errorMessage = $"Invalid DateOnly value for {{item.PropertyName}}.  Trying to use {value}";
                            """)
                    .WriteLine("return false;");
                });
            return w;
        }
        if (item.VariableCustomCategory == EnumSimpleTypeCategory.DateTime)
        {
            string lower = item.PropertyName.ChangeCasingForVariable(EnumVariableCategory.ParameterCamelCase);
            w.WriteLine($"if (DateTime.TryParse(value, out DateTime {lower}))")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine($"item.{item.PropertyName} = {lower};");
                    w.ShowTrue();
                })
                .WriteLine("else")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine($$"""
                            errorMessage = $"Invalid DateTime value for {{item.PropertyName}}.  Trying to use {value}";
                            """)
                    .WriteLine("return false;");
                });
            return w;
        }
        if (item.VariableCustomCategory == EnumSimpleTypeCategory.DateTimeOffset)
        {
            string lower = item.PropertyName.ChangeCasingForVariable(EnumVariableCategory.ParameterCamelCase);
            w.WriteLine($"if (DateTimeOffset.TryParse(value, out DateTimeOffset {lower}))")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine($"item.{item.PropertyName} = {lower};");
                    w.ShowTrue();
                })
                .WriteLine("else")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine($$"""
                            errorMessage = $"Invalid DateTimeOffset value for {{item.PropertyName}}.  Trying to use {value}";
                            """)
                    .WriteLine("return false;");
                });
            return w;
        }
        if (item.VariableCustomCategory == EnumSimpleTypeCategory.TimeOnly)
        {
            string lower = item.PropertyName.ChangeCasingForVariable(EnumVariableCategory.ParameterCamelCase);
            w.WriteLine($"if (TimeOnly.TryParse(value, out TimeOnly {lower}))")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine($"item.{item.PropertyName} = {lower};");
                    w.ShowTrue();
                })
                .WriteLine("else")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine($$"""
                            errorMessage = $"Invalid TimeOnly value for {{item.PropertyName}}.  Trying to use {value}";
                            """)
                    .WriteLine("return false;");
                });
            return w;
        }
        if (item.VariableCustomCategory == EnumSimpleTypeCategory.TimeSpan)
        {
            string lower = item.PropertyName.ChangeCasingForVariable(EnumVariableCategory.ParameterCamelCase);
            w.WriteLine($"if (TimeSpan.TryParse(value, out TimeSpan {lower}))")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine($"item.{item.PropertyName} = {lower};");
                    w.ShowTrue();
                })
                .WriteLine("else")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine($$"""
                            errorMessage = $"Invalid TimeSpan value for {{item.PropertyName}}.  Trying to use {value}";
                            """)
                    .WriteLine("return false;");
                });
            return w;
        }
        return w;
    }
    private static ICodeBlock ShowTrue(this ICodeBlock w)
    {
        w.WriteLine("errorMessage = null;");
        w.WriteLine("return true;");
        return w;
    }
}