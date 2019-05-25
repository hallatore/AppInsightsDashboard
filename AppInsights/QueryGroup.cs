using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AppInsights
{
    public class QueryGroup
    {
        private readonly Dictionary<string, StructuredQuery> _queries;
        private readonly Dictionary<string, bool> _queriesUsed;
        private readonly string _duration;

        private StructuredQuery _query;

        public QueryGroup(
            StructuredQuery query,
            string duration)
        {
            _query = query;
            _duration = duration;
            _queries = new Dictionary<string, StructuredQuery>();
            _queriesUsed = new Dictionary<string, bool>();

            foreach (var type in new[] { "requests", "exceptions", "availabilityResults" })
            {
                _queries.Add(type, QueryBuilder.Parse($" {type} | where timestamp > ago(1h)"));
                _queriesUsed.Add(type, false);
            }
        }

        public void AddParts(string[] queryParts)
        {
            foreach (var part in queryParts)
            {
                if (part.StartsWith("where type ", StringComparison.OrdinalIgnoreCase))
                {
                    if (IsExceptionsQuery())
                    {
                        AddWhere(part);
                    }
                    else
                    {
                        AddWhere("exceptions", part);
                    }
                }
                else if (part.StartsWith("where operation_Name ", StringComparison.OrdinalIgnoreCase) ||
                         part.StartsWith("where url ", StringComparison.OrdinalIgnoreCase) ||
                         part.StartsWith("where resultCode ", StringComparison.OrdinalIgnoreCase))
                {
                    if (IsRequestsQuery())
                    {
                        AddWhere(part);
                    }
                    else
                    {
                        AddWhere("requests", part);
                    }
                }
            }
        }

        public void AddWhere(string type, string part)
        {
            _queries[type] = QueryBuilder.AddWherePart(_queries[type], part);
            _queriesUsed[type] = true;
        }

        public void Replace(string type, StructuredQuery query)
        {
            _queries[type] = query;
            _queriesUsed[type] = true;
        }

        public void AddWhere(string part)
        {
            _query = QueryBuilder.AddWherePart(_query, part);
        }

        public void Append(StructuredQuery query)
        {
            _query = QueryBuilder.Append(_query, query);
        }

        public void RemoveProjectAndSummarize()
        {
            _query = QueryBuilder.RemoveProject(_query);
            _query = QueryBuilder.RemoveSummarize(_query);
        }

        public bool IsRequestsQuery()
        {
            return _query.FirstOrDefault()?.Trim().StartsWith("requests") == true;
        }

        public bool IsExceptionsQuery()
        {
            return _query.FirstOrDefault()?.Trim().StartsWith("exceptions") == true;
        }

        public bool IsAvailabilityResultsQuery()
        {
            return _query.FirstOrDefault()?.Trim().StartsWith("availabilityResults") == true;
        }

        public override string ToString()
        {
            var result = "";
            var query = _query;

            foreach (var queryUsed in _queriesUsed)
            {
                if (!queryUsed.Value)
                {
                    continue;
                }

                var extrQuery = _queries[queryUsed.Key];
                var whereParts = extrQuery
                    .Where(part => part.StartsWith("where") && (part.Contains("==") || part.Contains("!=") || part.Contains("contains")))
                    .ToList();

                var inString = "in";
                var invertPart = whereParts.All(part => part.Contains("!=") || part.Contains("!contains")) && whereParts.Count > 0;

                // We invert the query to avoid hiding un-correlated items like non failed requests.
                if (invertPart)
                {
                    inString = "!in";
                    extrQuery = new StructuredQuery(extrQuery);
                    var tmpWhere = new List<string>();

                    for (var i = 0; i < extrQuery.Count; i++)
                    {
                        var part = extrQuery[i];

                        if (part.StartsWith("where") && (part.Contains("==") || part.Contains("!=")))
                        {
                            tmpWhere.Add(
                                part
                                    .Replace("where", "")
                                    .Replace("!=", "==")
                                    .Replace("!contains", "contains")
                                    .Trim());

                            extrQuery.RemoveAt(i);
                            i--;
                        }
                    }

                    extrQuery = QueryBuilder.AddWherePart(extrQuery, $"where {string.Join(" or ", tmpWhere)}");
                }

                var tmpQuery = QueryBuilder.Append(extrQuery, QueryBuilder.Parse("| distinct operation_Id"));
                result += $"let {queryUsed.Key}OperationIds = {QueryBuilder.ToQueryString(tmpQuery)};\r\n";
                query = QueryBuilder.AddWherePart(query, $"where (operation_Id {inString} ({queryUsed.Key}OperationIds))");
            }

            result += QueryBuilder.ToQueryString(query);
            result = Regex.Replace(result, @"ago\(([0-9a-z]+)\)", $"ago({_duration})");
            result = Regex.Replace(result, "[ ]+", " ");
            return result;
        }
    }
}