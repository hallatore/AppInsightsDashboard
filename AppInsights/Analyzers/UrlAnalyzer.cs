using System;
using System.Linq;
using System.Threading.Tasks;

namespace AppInsights.Analyzers
{
    public class UrlAnalyzer
    {
        public static async Task<IAnalyzerResult> Analyze(Guid appid, string apikey, StructuredQuery query, string[] queryParts)
        {
            query = QueryBuilder.RemoveProject(query);
            query = QueryBuilder.RemoveSummarize(query);
            var duration = QueryBuilder.GetDuration(query);
            var isRequestsQuery = query.FirstOrDefault()?.Trim().StartsWith("requests", StringComparison.OrdinalIgnoreCase) == true;
            QueryGroup queryGroup;

            var whereQuery = @"
                | summarize duration = avg(duration), failedCount=sumif(itemCount, success == false), totalCount=sum(itemCount) by url
                | order by failedCount desc
                | take 20
                | project url, totalCount, duration, failedCount, failedPercentage = 100.0 / totalCount * failedCount";

            if (isRequestsQuery)
            {
                queryGroup = new QueryGroup(query, duration);
                queryGroup.AddParts(queryParts);
                queryGroup.Append(QueryBuilder.Parse(whereQuery));
            }
            else
            {
                var requestsQuery = QueryBuilder.Parse($@"
                requests
                | where timestamp > ago(1h)
                {whereQuery}");
                queryGroup = new QueryGroup(requestsQuery, duration);
                queryGroup.Replace(query.First().Trim(), query);
                queryGroup.AddParts(queryParts);
            }

            var queryString = queryGroup.ToString();
            var result = await AppInsightsClient.GetTableQuery(appid, apikey, queryString);
            result.Columns[0].Name = "Url";
            result.Columns[1].Name = "Count";
            result.Columns[2].Name = "Duration";
            result.Columns[3].Name = "Failures";
            result.Columns[4].Name = "Percentage";

            foreach (var row in result.Rows)
            {
                row[2] = $"{row[2]:0} ms";
                row[4] = $"{row[4]:0} %";
                row.Add($"where url == '{row[0]}'");
                row.Add($"where url != '{row[0]}'");
            }
            
            return new TableAnalyzerResult("Urls", true, result);
        }
    }
}