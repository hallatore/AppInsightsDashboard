using System;
using System.Linq;
using AppInsights;

namespace ConfigLogic.Dashboard
{
    public class DashboardItem
    {
        public string Name { get; set; }
        public ApiToken ApiToken { get; set; }
        public string Query { get; set; }
        public string Postfix { get; set; }
        public ItemDuration Duration { get; set; } = ItemDuration.OneHour;
        public ItemTotal Total { get; set; } = ItemTotal.Average;
        public double MinChartValue { get; set; } = 100;
        public double DisabledThreshold { get; set; }
        public double WarningThreshold { get; set; }
        public double ErrorThreshold { get; set; }
        public int StatusSplitFactor { get; set; } = 4;
        public Func<double, string> FormatValue { get; set; } = s => s.ToString("0");
        public Func<DashboardItem, double, TableResult, ItemStatus> GetStatus = _GetStatus;

        private static ItemStatus _GetStatus(DashboardItem item, double value, TableResult table)
        {
            if (value <= item.DisabledThreshold)
            {
                return ItemStatus.Disabled;
            }

            if (table.Rows.Count > item.StatusSplitFactor)
            {
                var tmpQuery = table.Rows.OrderByDescending(v => (DateTime)v[0]).AsEnumerable();
                var finalTmpValue = 0.0;

                if (item.Duration.GetIntervalString().GetTimeSpan() < TimeSpan.FromMinutes(10))
                {
                    tmpQuery = tmpQuery.Skip(1);
                }

                if (item.StatusSplitFactor > 0)
                {
                    var count = tmpQuery.Count();
                    tmpQuery = tmpQuery.Take(count / item.StatusSplitFactor);
                }

                if (item.Total == ItemTotal.Sum)
                {
                    finalTmpValue = tmpQuery.Sum(v => (double)v[1]);
                }
                else if (item.Total == ItemTotal.Average)
                {
                    finalTmpValue = tmpQuery.Average(v => (double)v[1]);
                }
                else
                {
                    return ItemStatus.Normal;
                }

                if (finalTmpValue >= item.ErrorThreshold && item.ErrorThreshold > 0)
                {
                    return ItemStatus.Error;
                }
                else if (finalTmpValue >= item.WarningThreshold && item.WarningThreshold > 0)
                {
                    return ItemStatus.Warning;
                }
            }

            return ItemStatus.Normal;
        }

        public static DashboardItem AddRequestPerMinute(string name = null, Action<DashboardItem> options = null, string whereQuery = null)
        {
            var item = new DashboardItem
            {
                Name = name ?? "Requests",
                Query = $@"
                    requests
                    | where timestamp > ago(1h)
                    | where client_Type == 'PC'
                    {whereQuery}
                    | summarize _count=sum(itemCount) by bin(timestamp, 2m)
                    | project timestamp, _count",
                Postfix = "pm",
                Total = ItemTotal.Rpm,
                MinChartValue = 1000
            };

            options?.Invoke(item);
            return item;
        }

        public static DashboardItem AddExceptionPerMinute(string name = null, Action<DashboardItem> options = null, string whereQuery = null)
        {
            var item = new DashboardItem
            {
                Name = name ?? "Exceptions",
                Query = $@"
                    exceptions
                    | where timestamp > ago(1h)
                    | where client_Type == 'PC'
                    {whereQuery}
                    | summarize _count=sum(itemCount) by bin(timestamp, 2m)
                    | project timestamp, _count",
                Postfix = "pm",
                Total = ItemTotal.Rpm
            };

            options?.Invoke(item);
            return item;
        }

        public static DashboardItem AddRequestResponseTime(string name = null, Action<DashboardItem> options = null, string whereQuery = null, DurationType durationType = DurationType.Percentile_90)
        {
            var averageString = "avg(duration)";

            switch (durationType)
            {
                case DurationType.Percentile_50:
                    averageString = "percentile(duration, 50)";
                    break;
                case DurationType.Percentile_90:
                    averageString = "percentile(duration, 90)";
                    break;
                case DurationType.Percentile_95:
                    averageString = "percentile(duration, 95)";
                    break;
                case DurationType.Percentile_99:
                    averageString = "percentile(duration, 99)";
                    break;
            }

            var item = new DashboardItem
            {
                Name = name ?? "Response time",
                Query = $@"
                    requests
                    | where timestamp > ago(1h)
                    | where client_Type == 'PC'
                    {whereQuery}
                    | summarize _duration = {averageString} by bin(timestamp, 2m)
                    | project timestamp, _duration",
                Postfix = "ms",
                Total = ItemTotal.Average,
                MinChartValue = 1000,
                WarningThreshold = 1000,
                ErrorThreshold = 2000
            };

            options?.Invoke(item);
            return item;
        }

        public static DashboardItem AddFailedRequestsPercentage(string name = null, Action<DashboardItem> options = null, string whereQuery = null)
        {
            var item = new DashboardItem
            {
                Name = name ?? "Failed requests",
                Query = $@"
                    requests
                    | where timestamp > ago(1h)
                    | where client_Type == 'PC'
                    {whereQuery}
                    | summarize totalCount=sum(itemCount), errorCount=sumif(itemCount, success == false)   by bin(timestamp, 2m)
                    | project timestamp, 100.0 / totalCount * errorCount",
                Postfix = "%",
                MinChartValue = 10,
                FormatValue = d => d.ToString("0.#"),
                WarningThreshold = 5,
                ErrorThreshold = 10
            };

            options?.Invoke(item);
            return item;
        }

        public static DashboardItem AddExceptionsWhere(string name = null, Action<DashboardItem> options = null, string whereQuery = null)
        {
            var item = new DashboardItem
            {
                Name = name ?? "Exceptions",
                Query = $@"
                    exceptions
                    | where timestamp > ago(1h)
                    | where client_Type == 'PC'
                    {whereQuery}
                    | summarize _count=sum(itemCount) by bin(timestamp, 2m)
                    | project timestamp, _count",
                MinChartValue = 10,
                Total = ItemTotal.Sum,
                WarningThreshold = 10,
                ErrorThreshold = 50
            };

            options?.Invoke(item);
            return item;
        }

        public static DashboardItem AddWebTestsPercentage(string name = null, Action<DashboardItem> options = null, string whereQuery = null)
        {
            var item = new DashboardItem
            {
                Name = name ?? "Web tests",
                Query = $@"
                    availabilityResults
                    | where timestamp > ago(24h)
                    | where client_Type == 'PC'
                    {whereQuery}
                    | summarize _successCount=todouble(countif(success == 1)), _totalCount=todouble(count()) by bin(timestamp, 1h)
                    | project timestamp, 100.0 - (_successCount / _totalCount * 100.0)",
                Postfix = "%",
                MinChartValue = 10,
                Duration = ItemDuration.TwelveHours,
                FormatValue = d => (100.0 - d).ToString("0.#"),
                WarningThreshold = 1,
                ErrorThreshold = 5
            };

            options?.Invoke(item);
            return item;
        }
    }
}