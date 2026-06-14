namespace OpenPlaDiC.Core.Models.DynamicQuery;

public enum FilterOperator
{
    Equals,
    Contains,
    Between,
    In
}

// Usamos C# 10+ Primary Constructors para un record inmutable y limpio
public record FilterCriterion(
    string PropertyName, 
    FilterOperator Operator, 
    object? Value1, 
    object? Value2 = null
);