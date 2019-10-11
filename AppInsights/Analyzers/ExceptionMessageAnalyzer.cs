using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AppInsights.Analyzers
{
    public class ExceptionMessageAnalyzer
    {
        public static async Task<IAnalyzerResult> Analyze(Guid appid, string apikey, StructuredQuery query, string[] queryParts)
        {
            query = QueryBuilder.RemoveProject(query);
            query = QueryBuilder.RemoveSummarize(query);
            var duration = QueryBuilder.GetDuration(query);
            var isExceptionsQuery = query.FirstOrDefault()?.Trim().StartsWith("exceptions", StringComparison.OrdinalIgnoreCase) == true;
            QueryGroup queryGroup;

            var whereQuery = @"
                | project itemCount, message = strcat(outerMessage, ' \n\n ', customDimensions['AdditionalErrorDetails'], ' \n\n ', customDimensions['additionalDetails'])
                | project itemCount, message = replace(@'[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}', '[GUID]', message) 
                | project itemCount, message = replace(@'(^|[ ''])(http:|https:)*[/]{1,2}(.*?)([ ]|$|\n)', ' [URL] ', message)
                | project itemCount, message = replace(@'[0-9]', '[X]', message)
                | where message !contains 'username' or message !contains 'password' 
                | summarize sum(itemCount) by message
                | order by sum_itemCount desc
                | take 20";

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
            result.Columns[0].Name = "Exception message";
            result.Columns[1].Name = "Count";

            foreach (var row in result.Rows)
            {
                var rowText = (string)row[0];
                var commandText = ((string)row[0]).Trim();

                rowText = rowText.Replace("[X]", "X");
                rowText = Regex.Replace(rowText, "[X]+", "X");

                var lines = new List<string>();

                foreach (var line in Regex.Split(rowText, @"\n").SelectMany(line => Regex.Split(Regex.Replace(line, @"\. ", ".. "), @"\. ")))
                {
                    var trimmedLine = Regex.Replace(line, @"^[ ]*\[[A-Z]+\][ ]*", "").Trim();
                    if (!string.IsNullOrWhiteSpace(trimmedLine) && lines.All(l => l != trimmedLine))
                    {
                        lines.Add(trimmedLine);
                    }
                }

                rowText = string.Join("\n", lines);
                row[0] = rowText.Trim();

                var matches = Regex.Matches(commandText, @"(^|\])(.*?)($|\[)");

                if (matches.Any(match => match.Groups[2].Length > 10))
                {
                    commandText = matches
                        .First(match => match.Groups[2].Length > 10)
                        .Groups[2].Value.Trim();
                }
                

                row.Add($"where outerMessage contains '{commandText}' or customDimensions['AdditionalErrorDetails'] contains '{commandText}' or customDimensions['additionalDetails'] contains '{commandText}'");
                row.Add($"where outerMessage !contains '{commandText}' and customDimensions['AdditionalErrorDetails'] !contains '{commandText}' and customDimensions['additionalDetails'] !contains '{commandText}'");
            }

            return new TableAnalyzerResult("Exception messages", true, result);
        }
    }
}