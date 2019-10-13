using System;
using System.Linq;
using System.Threading.Tasks;

namespace AppInsights.Analyzers
{
    public class StatusCodesAnalyzer
    {
        public static async Task<IAnalyzerResult> Analyze(Guid appid, string apikey, StructuredQuery query, string[] queryParts)
        {
            query = QueryBuilder.RemoveProject(query);
            query = QueryBuilder.RemoveSummarize(query);
            var isRequestsQuery = query.FirstOrDefault()?.Trim().StartsWith("requests", StringComparison.OrdinalIgnoreCase) == true;
            QueryGroup queryGroup;

            var whereQuery = @"
                | summarize _count=sum(itemCount) by resultCode
                | project resultCode, _count
                | sort by _count desc";

            if (isRequestsQuery)
            {
                queryGroup = new QueryGroup(query);
                queryGroup.AddParts(queryParts);
                queryGroup.Append(QueryBuilder.Parse(whereQuery));
            }
            else
            {
                var requestsQuery = QueryBuilder.Parse(
                    $@"
                requests
                | {query.First(q => q.Contains("timestamp >"))}
                {whereQuery}");

                queryGroup = new QueryGroup(requestsQuery);
                queryGroup.Replace(query.First().Trim(), query);
                queryGroup.AddParts(queryParts);
            }

            var queryString = queryGroup.ToString();
            var result = await AppInsightsClient.GetTableQuery(appid, apikey, queryString);
            result.Columns[0].Name = "Status code";
            result.Columns[1].Name = "Count";

            foreach (var row in result.Rows)
            {
                row.Add($"where resultCode == {row[0]}");
                row.Add($"where resultCode != {row[0]}");
            }

            return new TableAnalyzerResult("Status codes", true, result);
        }
    }
}