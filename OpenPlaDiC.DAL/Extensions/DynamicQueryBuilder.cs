using System.Text;
using Dapper;
using OpenPlaDiC.Core.Models.DynamicQuery;
using OpenPlaDiC.Framework;

namespace OpenPlaDiC.DAL.Extensions;

public static class DynamicQueryBuilder
{
    public static (string SqlWhereClause, DynamicParameters Parameters) BuildWhereClause(
        HashSet<FilterCriterion> criteria, 
        Dictionary<string, int> propertyTypes) // propertyTypes mapea: NombrePropiedad -> TipoDatoKernel
    {
        if (criteria == null || !criteria.Any())
            return (string.Empty, new DynamicParameters());

        var sb = new StringBuilder(" WHERE IsDeleted = 0 ");
        var parameters = new DynamicParameters();
        int paramCounter = 0;

        foreach (var criterion in criteria)
        {
            paramCounter++;
            string paramName1 = $"@p_{criterion.PropertyName}_{paramCounter}_1";
            string paramName2 = $"@p_{criterion.PropertyName}_{paramCounter}_2";

            // Sanitización estricta del nombre de la columna para evitar inyección en identificadores
            string safeColumnName = $"[{criterion.PropertyName.Replace("]", "]]")}]";

            switch (criterion.Operator)
            {
                case FilterOperator.Equals:
                    if (criterion.Value1 == null || criterion.Value1 == DBNull.Value) continue;
                    sb.Append($" AND {safeColumnName} = {paramName1}");
                    parameters.Add(paramName1, criterion.Value1);
                    break;

                case FilterOperator.Contains:
                    if (criterion.Value1 is not string strVal || string.IsNullOrWhiteSpace(strVal)) continue;
                    sb.Append($" AND {safeColumnName} LIKE {paramName1}");
                    parameters.Add(paramName1, $"%{strVal}%");
                    break;

                case FilterOperator.Between:
                    if (criterion.Value1 == null || criterion.Value2 == null) continue;
                    sb.Append($" AND {safeColumnName} BETWEEN {paramName1} AND {paramName2}");
                    parameters.Add(paramName1, criterion.Value1);
                    parameters.Add(paramName2, criterion.Value2);
                    break;

                case FilterOperator.In:
                    if (criterion.Value1 is not IEnumerable<object> list || !list.Any()) continue;
                    sb.Append($" AND {safeColumnName} IN {paramName1}");
                    parameters.Add(paramName1, list); // Dapper maneja colecciones IN de forma nativa
                    break;
            }
        }

        return (sb.ToString(), parameters);
    }

    // Dentro de tu flujo de generación de SQL en el servicio o Builder:
    public static (string SqlClause, List<GlobalItem> DbParameters) BuildAdvancedWhereClause(
        List<GlobalItem> uiFilters, 
        Dictionary<string, int> propertyTypes)
    {
        var sqlBuilder = new StringBuilder();
        var dbParameters = new List<GlobalItem>();
        int paramCounter = 0;

        foreach (var filter in uiFilters)
        {
            paramCounter++;
            string p1 = $"@p_adv_{paramCounter}_v1";
            string p2 = $"@p_adv_{paramCounter}_v2";

            string columnName = $"[{filter.Name}]";
            string op = filter.Opt.ToLower();

            switch (op)
            {
                // === OPERADORES DE STRING ===
                case "contains":
                    sqlBuilder.Append($" AND {columnName} LIKE {p1}");
                    dbParameters.Add(new GlobalItem(p1.Replace("@",""), $"%{filter.Value}%"));
                    break;
                case "startswith":
                    sqlBuilder.Append($" AND {columnName} LIKE {p1}");
                    dbParameters.Add(new GlobalItem(p1.Replace("@",""), $"{filter.Value}%"));
                    break;
                case "endswith":
                    sqlBuilder.Append($" AND {columnName} LIKE {p1}");
                    dbParameters.Add(new GlobalItem(p1.Replace("@",""), $"%{filter.Value}"));
                    break;
                case "notcontains":
                    sqlBuilder.Append($" AND {columnName} NOT LIKE {p1}");
                    dbParameters.Add(new GlobalItem(p1.Replace("@",""), $"%{filter.Value}%"));
                    break;

                // === OPERADORES COMUNES (NUMÉRICOS, FECHAS, IGUALDAD) ===
                case "equals":
                case "equalto":
                    sqlBuilder.Append($" AND {columnName} = {p1}");
                    dbParameters.Add(new GlobalItem(p1.Replace("@",""), filter.Value));
                    break;
                case "notequals":
                    sqlBuilder.Append($" AND {columnName} <> {p1}");
                    dbParameters.Add(new GlobalItem(p1.Replace("@",""), filter.Value));
                    break;
                case "greaterthan":
                    sqlBuilder.Append($" AND {columnName} > {p1}");
                    dbParameters.Add(new GlobalItem(p1.Replace("@",""), filter.Value));
                    break;
                case "lessthan":
                    sqlBuilder.Append($" AND {columnName} < {p1}");
                    dbParameters.Add(new GlobalItem(p1.Replace("@",""), filter.Value));
                    break;
                case "greaterthanorequal":
                    sqlBuilder.Append($" AND {columnName} >= {p1}");
                    dbParameters.Add(new GlobalItem(p1.Replace("@",""), filter.Value));
                    break;
                case "lessthanorequal":
                    sqlBuilder.Append($" AND {columnName} <= {p1}");
                    dbParameters.Add(new GlobalItem(p1.Replace("@",""), filter.Value));
                    break;

                // === OPERADORES DE RANGOS Y FECHAS ESPECIALES ===
                case "between":
                    sqlBuilder.Append($" AND {columnName} BETWEEN {p1} AND {p2}");
                    dbParameters.Add(new GlobalItem(p1.Replace("@",""), filter.Value));
                    dbParameters.Add(new GlobalItem(p2.Replace("@",""), filter.Text)); // Ocupamos la propiedad Text
                    break;

                case "today":
                    sqlBuilder.Append($" AND CAST({columnName} AS DATE) = CAST(GETDATE() AS DATE)");
                    break;
                case "yesterday":
                    sqlBuilder.Append($" AND CAST({columnName} AS DATE) = CAST(DATEADD(day, -1, GETDATE()) AS DATE)");
                    break;
                case "thismonth":
                    sqlBuilder.Append($" AND YEAR({columnName}) = YEAR(GETDATE()) AND MONTH({columnName}) = MONTH(GETDATE())");
                    break;

                // === BOOLEANOS ===
                case "active":
                    sqlBuilder.Append($" AND {columnName} = 1");
                    break;
                case "inactive":
                    sqlBuilder.Append($" AND ({columnName} = 0 OR {columnName} IS NULL)");
                    break;
            }
        }

        return (sqlBuilder.ToString(), dbParameters);
    }


}