using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace AppInsights
{
    public static class QueryBuilder
    {
        public static StructuredQuery Parse(string query)
        {
            var parts = query.Split("| ", StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Trim())
                .Where(part => !string.IsNullOrWhiteSpace(part));

            return new StructuredQuery(parts);
        }

        public static StructuredQuery RemoveSummarize(StructuredQuery structuredQuery)
        {
            return new StructuredQuery(structuredQuery.Where(part => !part.StartsWith("summarize ", StringComparison.OrdinalIgnoreCase)));
        }

        public static StructuredQuery RemoveProject(StructuredQuery structuredQuery)
        {
            return new StructuredQuery(structuredQuery.Where(part => !part.StartsWith("project ", StringComparison.OrdinalIgnoreCase)));
        }

        public static string ToQueryString(StructuredQuery structuredQuery)
        {
            return string.Join("\r\n| ", structuredQuery);
        }

        public static StructuredQuery Append(StructuredQuery structuredQuery, StructuredQuery parts)
        {
            return new StructuredQuery(structuredQuery.Concat(parts));
        }

        public static StructuredQuery AddWherePart(StructuredQuery structuredQuery, string part)
        {
            var firstWhereIndex = 0;

            for (var i = 0; i < structuredQuery.Count; i++)
            {
                if (structuredQuery[i].StartsWith("where ", StringComparison.OrdinalIgnoreCase))
                {
                    firstWhereIndex = i;
                    break;
                }
            }

            var result = new StructuredQuery(structuredQuery);
            result.Insert(firstWhereIndex, part);
            return result;
        }
    }
}