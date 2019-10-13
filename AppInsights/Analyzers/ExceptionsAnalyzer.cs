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
            var isExceptionsQuery = query.FirstOrDefault()?.Trim().StartsWith("exceptions", StringComparison.OrdinalIgnoreCase) == true;
            QueryGroup queryGroup;

            var whereQuery = @"
                | summarize _count = sum(itemCount) by type
                | sort by _count desc
                | take 20";

            if (isExceptionsQuery)
            {
                queryGroup = new QueryGroup(query);
                queryGroup.AddParts(queryParts);
                queryGroup.Append(QueryBuilder.Parse(whereQuery));
            }
            else
            {
                var exceptionsQuery = QueryBuilder.Parse(
                    $@"
                exceptions
                | {query.First(q => q.Contains("timestamp >"))}
                {whereQuery}");

                queryGroup = new QueryGroup(exceptionsQuery);
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