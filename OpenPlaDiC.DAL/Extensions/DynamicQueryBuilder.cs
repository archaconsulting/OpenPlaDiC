using System.Text;
using Dapper;
using OpenPlaDiC.Core.Models.DynamicQuery;

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
}