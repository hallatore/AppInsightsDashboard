using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AppInsights;
using AppInsights.Analyzers;
using ConfigLogic;
using ConfigLogic.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace AppInsightsDashboard.Controllers
{
    [Route("api/[controller]/{dashboardId}")]
    public class DashboardController : Controller
    {
        private readonly Config _config;

        public DashboardController(Config config)
        {
            _config = config;
        }

        [HttpGet("")]
        public IEnumerable<DashboardGroup> Dashboard(Guid dashboardId)
        {
            if (!_config.Dashboards.ContainsKey(dashboardId))
            {
                return Enumerable.Empty<DashboardGroup>();
            }

            return _config.Dashboards[dashboardId]
                .Select(
                    group => new DashboardGroup(
                        group.Key,
                        group.Value.Select(item => new DashboardItem(item.Name, item.Postfix))
                    )
                );
        }

        [HttpGet("Overview/{groupIndex}/{itemIndex}")]
        public async Task<dynamic> Overview(Guid dashboardId, int groupIndex, int itemIndex)
        {
            var item = _config.Dashboards[dashboardId].Select(d => d.Value).ToList()[groupIndex][itemIndex];
            var itemQuery = GetQueryString(item, item.Duration.GetString(), item.Duration.GetIntervalString());
            var table = await AppInsightsClient.GetTableQuery(item.ApiToken, itemQuery);
            var values = table.Rows.Select(row => new RowItem(row[0], row[1])).ToList();
            var value = await GetValueQuery(item, GetQueryString(item, item.Duration.GetString(), "90d"));

            var chart = TransformQuery(values, item.Duration.GetString().GetTimeSpan(), item.Duration.GetIntervalString().GetTimeSpan());
            var max = Math.Max(item.MinChartValue, chart.Any() ? chart.Max() : 0);
            var status = item.GetStatus(item, value, chart);

            return new
            {
                Value = item.FormatValue(value),
                ChartValues = chart,
                ChartMax = max,
                Status = status
            };
        }

        [HttpGet("Details/{groupIndex}/{itemIndex}")]
        public async Task<dynamic> Details(Guid dashboardId, int groupIndex, int itemIndex, ItemDuration duration, DateTime durationFrom, DateTime durationTo, string searchQuery, string[] queryParts)
        {
            var dashboard = _config.Dashboards[dashboardId];
            var groupKey = dashboard.Keys.ToList()[groupIndex];
            var item = _config.Dashboards[dashboardId].Select(d => d.Value).ToList()[groupIndex][itemIndex];
            string itemQuery;
            string intervalString;

            if (duration == ItemDuration.Custom)
            {
                intervalString = durationFrom.GetIntervalString(durationTo);
                itemQuery = GetQueryString(item, durationFrom, durationTo, intervalString);
            }
            else
            {
                intervalString = duration.GetIntervalString();
                itemQuery = GetQueryString(item, duration.GetString(), intervalString);
            }

            var structuredQuery = QueryBuilder.Parse(itemQuery);
            var queryGroup = new QueryGroup(structuredQuery);
            queryGroup.AddParts(queryParts);

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                foreach (var searchQueryPart in GetSearchQueryParts(searchQuery))
                {
                    queryGroup.AddWhere(searchQueryPart);
                }
            }

            var values = await GetChartValues(item, queryGroup.ToString());

            // Fallback when all values are errors
            if ((queryGroup.IsRequestsQuery() || queryGroup.IsExceptionsQuery()) &&
                values.Count > 4 &&
                (int) values.OrderByDescending(v => v.Value).Skip(values.Count / 2).Take(values.Count / 2).Max(v => v.Value) ==
                (int) values.OrderByDescending(v => v.Value).Skip(values.Count / 2).Take(values.Count / 2).Average(v => v.Value))
            {
                queryGroup.RemoveProjectAndSummarize();
                queryGroup.Append(QueryBuilder.Parse($"summarize _count=sum(itemCount) by bin(timestamp, {intervalString}) | project timestamp, _count"));
                values = await GetChartValues(item, queryGroup.ToString());
            }

            List<(DateTime Date, double Value)> chart;

            if (duration == ItemDuration.Custom)
            {
                chart = FillEmptySlotsInTimeRange(values, durationFrom, durationTo, intervalString.GetTimeSpan());
            }
            else
            {
                chart = FillEmptySlotsInTimeRange(values, duration.GetString().GetTimeSpan(), intervalString.GetTimeSpan());
            }

            var chartMax = chart.Any() ? chart.Max(c => c.Value) : 0;
            var minMax = item.MinChartValue / item.Duration.GetIntervalString().GetTimeSpan().TotalMinutes * intervalString.GetTimeSpan().TotalMinutes;
            minMax = Math.Min(minMax, chartMax * 5);
            var max = Math.Max(minMax, chartMax);

            queryGroup.RemoveProjectAndSummarize();
            var count = await GetCountQuery(dashboardId, groupIndex, itemIndex, duration, durationFrom, durationTo, queryParts);

            return new
            {
                Name = $"{groupKey} / {item.Name}",
                ChartValues = chart.Select(c => new { c.Date, c.Value }),
                ChartMax = max,
                Query = queryGroup.ToString(),
                Count = count
            };
        }

        [HttpGet("Analyzer/{groupIndex}/{itemIndex}/{analyzer}")]
        public async Task<dynamic> Analyzer(Guid dashboardId, int groupIndex, int itemIndex, string analyzer, ItemDuration duration, DateTime durationFrom, DateTime durationTo, string searchQuery, string[] queryParts)
        {
            var item = _config.Dashboards[dashboardId].Select(d => d.Value).ToList()[groupIndex][itemIndex];
            string itemQuery;

            if (duration == ItemDuration.Custom)
            {
                itemQuery = GetQueryString(item, durationFrom, durationTo, durationFrom.GetIntervalString(durationTo));
            }
            else
            {
                itemQuery = GetQueryString(item, duration.GetString(), duration.GetIntervalString());
            }

            var query = QueryBuilder.Parse(itemQuery);

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var queryGroup = new QueryGroup(query); 
                
                foreach (var searchQueryPart in GetSearchQueryParts(searchQuery))
                {
                    queryGroup.AddWhere(searchQueryPart);
                }

                query = QueryBuilder.Parse(queryGroup.ToString());
            }

            switch (analyzer)
            {
                case "RequestsAnalyzer":
                    return await RequestsAnalyzer.Analyze(item.ApiToken, query, queryParts);
                case "UrlAnalyzer":
                    return await UrlAnalyzer.Analyze(item.ApiToken, query, queryParts);
                case "DomainAnalyzer":
                    return await DomainAnalyzer.Analyze(item.ApiToken, query, queryParts);
                case "RoleAnalyzer":
                    return await RoleAnalyzer.Analyze(item.ApiToken, query, queryParts);
                case "StatusCodesAnalyzer":
                    return await StatusCodesAnalyzer.Analyze(item.ApiToken, query, queryParts);
                case "RequestExceptionsAnalyzer":
                    return await ExceptionsAnalyzer.Analyze(item.ApiToken, query, queryParts);
                case "StacktraceAnalyzer":
                    return await StacktraceAnalyzer.Analyze(item.ApiToken, query, queryParts);
                case "ExceptionMessageAnalyzer":
                    return await ExceptionMessageAnalyzer.Analyze(item.ApiToken, query, queryParts);
            }

            throw new InvalidOperationException($"Analyzer \"{analyzer}\" was not found.");
        }

        private IEnumerable<string> GetSearchQueryParts(string searchQuery)
        {
            var parts = searchQuery
                .Split(new[]{' '}, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            if (searchQuery.Contains("\""))
            {
                var insideLongString = false;
                parts = new List<string>();

                foreach (var bigPart in searchQuery.Split(new[] { '"' }))
                {
                    if (insideLongString)
                    {
                        parts.Add(bigPart);
                    }
                    else
                    {
                        parts.AddRange(bigPart.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                    }

                    insideLongString = !insideLongString;
                }
            }

            foreach (var searchQueryPart in parts)
            {
                var part = searchQueryPart.Trim().Replace(@"\", @"\\").Replace(@"'", @"\'");
                yield return part.StartsWith("!") ? $"where * !contains '{part.Substring(1)}'" : $"where * contains '{part}'";
            }
        }

        private async Task<double> GetValueQuery(ConfigLogic.Dashboard.DashboardItem item, string query)
        {
            var table = await AppInsightsClient.GetTableQuery(item.ApiToken, query);
            var value = 0.0;

            if (table.Rows.Count > 0 && table.Rows[0].Count > 1 && (table.Rows[0][1] is double || table.Rows[0][1] is long))
            {
                value = table.Rows[0][1];
            }

            if (item.Total == ItemTotal.Rpm)
            {
                return value / item.Duration.GetString().GetTimeSpan().TotalMinutes;
            }

            return value;
        }

        private async Task<long> GetCountQuery(Guid dashboardId, int groupIndex, int itemIndex, ItemDuration duration, DateTime durationFrom, DateTime durationTo, string[] queryParts)
        {
            var item = _config.Dashboards[dashboardId].Select(d => d.Value).ToList()[groupIndex][itemIndex];
            string itemQuery;

            if (duration == ItemDuration.Custom)
            {
                itemQuery = GetQueryString(item, durationFrom, durationTo, durationFrom.GetIntervalString(durationTo));
            }
            else
            {
                var durationString = duration.GetString();
                var intervalString = duration.GetIntervalString();
                itemQuery = GetQueryString(item, durationString, intervalString);
            }
            
            var structuredQuery = QueryBuilder.Parse(itemQuery);
            var queryGroup = new QueryGroup(structuredQuery);
            queryGroup.AddParts(queryParts);
            queryGroup.RemoveProjectAndSummarize();
            queryGroup.Append(QueryBuilder.Parse("| summarize sum(itemCount)"));
            var query = queryGroup.ToString();
            var table = await AppInsightsClient.GetTableQuery(item.ApiToken, query);
            return table.Rows.First()[0] as long? ?? 0;
        }

        private async Task<List<RowItem>> GetChartValues(ConfigLogic.Dashboard.DashboardItem item, string query)
        {
            var table = await AppInsightsClient.GetTableQuery(item.ApiToken, query);
            return table.Rows.Select(row => new RowItem(row[0], row[1])).ToList();
        }

        private string GetQueryString(ConfigLogic.Dashboard.DashboardItem item, string ago, string bin)
        {
            var query = item.Query.Trim();
            query = Regex.Replace(query, @"ago\([0-9a-z]+\)", $"ago({ago})");
            query = Regex.Replace(query, @"bin\(([\w]+),[ ]*[0-9a-z]+\)", $"bin($1, {bin})");
            query = Regex.Replace(query, @"\r\n[\W]*\|", "\r\n|");
            return query;
        }

        private string GetQueryString(ConfigLogic.Dashboard.DashboardItem item, DateTime durationFrom, DateTime durationTo, string bin)
        {
            var query = item.Query.Trim();
            query = Regex.Replace(query, @"> ago\([0-9a-z]+\)", $">= datetime('{(durationFrom.ToString("o"))}') and timestamp <= datetime('{(durationTo.ToString("o"))}')");
            query = Regex.Replace(query, @"bin\(([\w]+),[ ]*[0-9a-z]+\)", $"bin($1, {bin})");
            query = Regex.Replace(query, @"\r\n[\W]*\|", "\r\n|");
            return query;
        }

        private List<(DateTime Date, double Value)> FillEmptySlotsInTimeRange(List<RowItem> items, TimeSpan duration, TimeSpan interval)
        {
            if (items?.Any() == false)
            {
                return new List<(DateTime, double)>();
            }

            var maxDate = items.Max(c => c.Date);

            while (maxDate < DateTime.UtcNow - interval)
            {
                maxDate += interval;
            }

            var minDate = maxDate - duration;

            return FillEmptySlotsInTimeRange(items, minDate, maxDate, interval);
        }

        private List<(DateTime Date, double Value)> FillEmptySlotsInTimeRange(List<RowItem> items, DateTime durationFrom, DateTime durationTo, TimeSpan interval)
        {
            if (items?.Any() == false)
            {
                return new List<(DateTime, double)>();
            }

            var dictionary = items.ToDictionary(item => item.Date, item => item.Value);
            var minDateDifference = (int)Math.Floor((items[0].Date - durationFrom.ToUniversalTime()).TotalMinutes / interval.TotalMinutes);
            var minDate = items[0].Date.AddMinutes(minDateDifference * interval.TotalMinutes * -1);
            var maxDate = durationTo.ToUniversalTime();


            while (minDate <= maxDate)
            {
                if (!dictionary.ContainsKey(minDate))
                {
                    dictionary.Add(minDate, 0);
                }

                minDate += interval;
            }

            var result = dictionary
                .OrderBy(c => c.Key)
                .Select(c => (c.Key, Math.Round(c.Value, 1)))
                .ToList();

            if (result.Count > 10)
            {
                return result
                    .Skip(1)
                    .SkipLast(1)
                    .ToList();
            }

            return result;
        }

        private List<double> TransformQuery(List<RowItem> items, TimeSpan duration, TimeSpan interval)
        {
            if (items?.Any() == false)
            {
                return new List<double>();
            }

            var dictionary = items.ToDictionary(item => item.Date, item => item.Value);
            var maxDate = items.Max(c => c.Date);

            while (maxDate < DateTime.UtcNow - interval)
            {
                maxDate += interval;
            }

            var minDate = maxDate - duration;

            while (minDate <= maxDate)
            {
                if (!dictionary.ContainsKey(minDate))
                {
                    dictionary.Add(minDate, 0);
                }

                minDate += interval;
            }

            return dictionary
                .OrderBy(c => c.Key)
                .Select(c => Math.Round(c.Value, 1))
                .ToList();
        }
    }

    public class DashboardGroup
    {
        public DashboardGroup(string name, IEnumerable<DashboardItem> items)
        {
            Name = name;
            Items = items;
        }

        public string Name { get; }
        public IEnumerable<DashboardItem> Items { get; }
    }

    public class DashboardItem
    {
        public DashboardItem(string name, string postfix)
        {
            Name = name;
            Postfix = postfix;
        }

        public string Name { get; }
        public string Postfix { get; }
    }

    public class RowItem
    {
        public RowItem(DateTime date, double value)
        {
            Date = date;
            Value = value;
        }

        public DateTime Date { get; }
        public double Value { get; }
    }
}