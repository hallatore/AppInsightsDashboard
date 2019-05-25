using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AppInsights.Analyzers
{
    public class StacktraceAnalyzer
    {
        public static async Task<IAnalyzerResult> Analyze(Guid appid, string apikey, StructuredQuery query, string[] queryParts)
        {
            query = QueryBuilder.RemoveProject(query);
            query = QueryBuilder.RemoveSummarize(query);
            var duration = QueryBuilder.GetDuration(query);
            var isExceptionsQuery = query.FirstOrDefault()?.Trim().StartsWith("exceptions", StringComparison.OrdinalIgnoreCase) == true;
            QueryGroup queryGroup;

            var whereQuery = @"
                | project operation_Id, itemCount, type, stack = details[0].parsedStack[0]
                | project operation_Id, itemCount, filename = stack.fileName, type, line = stack.line
                | where filename != ''
                | summarize sum(itemCount) by tostring(filename), type, tostring(line)
                | order by sum_itemCount";

            if (isExceptionsQuery)
            {
                queryGroup = new QueryGroup(query, duration);
                queryGroup.AddParts(queryParts);
                queryGroup.Append(QueryBuilder.Parse(whereQuery));
            }
            else
            {
                var exceptionsQuery = QueryBuilder.Parse(
                    $@"
                exceptions
                | where timestamp > ago(1h)
                {whereQuery}");

                queryGroup = new QueryGroup(exceptionsQuery, duration);
                queryGroup.Replace(query.First().Trim(), query);
                queryGroup.AddParts(queryParts);
            }

            var queryString = queryGroup.ToString();
            var result = await AppInsightsClient.GetTableQuery(appid, apikey, queryString);
            result.Columns[0].Name = "File";
            result.Columns[1].Name = "Exception";
            result.Columns[2].Name = "Line";
            result.Columns[3].Name = "Count";

            foreach (var row in result.Rows)
            {
                row.Add($"where type == '{row[1]}'");
                row.Add($"where type != '{row[1]}'");

                row[0] = Path.GetFileName(row[0]);
                var exception = (string) row[1];
                row[1] = exception.Substring(exception.LastIndexOf('.') + 1);
            }

            return new TableAnalyzerResult("Stacktraces", true, result);
        }
    }
}