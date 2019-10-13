using System;
using System.Text.RegularExpressions;

namespace ConfigLogic.Dashboard
{
    public static class ItemDurationExtensions
    {
        public static string GetString(this ItemDuration duration)
        {
            switch (duration)
            {
                case ItemDuration.OneHour:
                    return "1h";
                case ItemDuration.SixHours:
                    return "6h";
                case ItemDuration.TwelveHours:
                    return "12h";
                case ItemDuration.OneDay:
                    return "1d";
                case ItemDuration.ThreeDays:
                    return "3d";
                case ItemDuration.SevenDays:
                    return "7d";
                case ItemDuration.ThirtyDays:
                    return "30d";
                default:
                    throw new ArgumentOutOfRangeException(nameof(duration), duration, null);
            }
        }

        public static string GetIntervalString(this ItemDuration duration, int splits = 30)
        {
            var minutes = Math.Max(duration.GetString().GetTimeSpan().TotalMinutes / splits, 1);
            return $"{minutes:0}m";
        }

        public static string GetIntervalString(this DateTime from, DateTime to, int splits = 30)
        {
            var minutes = Math.Max((to - from).TotalMinutes / splits, 1);
            return $"{minutes:0}m";
        }

        public static TimeSpan GetTimeSpan(this string input)
        {
            var match = Regex.Match(input, @"^([0-9]+)(m|h|d)$");

            if (!match.Success)
            {
                return TimeSpan.Zero;
            }

            switch (match.Groups[2].Value)
            {
                case "m":
                    return TimeSpan.FromMinutes(Convert.ToDouble(match.Groups[1].Value));
                case "h":
                    return TimeSpan.FromHours(Convert.ToDouble(match.Groups[1].Value));
                case "d":
                    return TimeSpan.FromDays(Convert.ToDouble(match.Groups[1].Value));
            }

            return TimeSpan.Zero;
        }
    }
}