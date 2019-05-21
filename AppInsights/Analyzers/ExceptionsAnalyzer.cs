using System;
using System.Linq;
using System.Threading.Tasks;

namespace AppInsights.Analyzers
{
    public class ExceptionsAnalyzer
    {
        public static async Task<IAnalyzerResult> Analyze(Guid appid, string apikey, StructuredQuery query, string[] queryParts)
        {
            query = QueryBuilder.RemoveProject(query);
            query = QueryBuilder.RemoveSummarize(query);
            var duration = QueryBuilder.GetDuration(query);
            var isExceptionsQuery = query.FirstOrDefault()?.Trim().StartsWith("exceptions", StringComparison.OrdinalIgnoreCase) == true;
            QueryGroup queryGroup;

            if (isExceptionsQuery)
            {
                queryGroup = new QueryGroup(query, duration);
                queryGroup.AddParts(queryParts);
                queryGroup.Append(QueryBuilder.Parse(@"
                | summarize _count = sum(itemCount) by type
                | sort by _count desc
                | take 20"));
            }
            else
            {
                var exceptionsQuery = QueryBuilder.Parse(@"
                exceptions
                | where timestamp > ago(1h)
                | summarize _count = sum(itemCount) by type
                | sort by _count desc
                | take 20");
                queryGroup = new QueryGroup(exceptionsQuery, duration);
                queryGroup.Replace(query.First().Trim(), query);
                queryGroup.AddParts(queryParts);
            }

            var queryString = queryGroup.ToString();
            var result = await AppInsightsClient.GetTableQuery(appid, apikey, queryString);
            result.Columns[0].Name = "Exception type";
            result.Columns[1].Name = "Count";

            foreach (var row in result.Rows)
            {
                row.Add($"where type == '{row[0]}'");
                row.Add($"where type != '{row[0]}'");
            }

            return new TableAnalyzerResult("Exceptions", true, result);
        }
    }
}