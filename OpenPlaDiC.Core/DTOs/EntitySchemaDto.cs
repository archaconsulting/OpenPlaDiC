namespace OpenPlaDiC.Core.DTOs;

public record PropertySchemaDto(
    string Name,
    string Label,
    int DataTypeId,
    string? SourceDefinition,
    bool IsRequired,
    bool IsUnique,
    bool IsIndexed,
    int GridRow,
    int GridColumn,
    bool OnList
);

public record EntitySchemaDto(
    string Name,
    string Label,
    string Prefix,
    string Icon,
    int PageSize,
    bool UseNameField,
    string NameLabel,
    List<PropertySchemaDto> Properties
);