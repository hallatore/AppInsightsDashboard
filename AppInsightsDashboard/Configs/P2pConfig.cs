using System;
using System.Collections.Generic;
using AppInsights;
using ConfigLogic;
using ConfigLogic.Dashboard;
using Microsoft.Extensions.Configuration;

namespace AppInsightsDashboard.Configs
{
    public class P2pConfig : IConfig
    {
        public (Guid id, Dictionary<string, DashboardItem[]> groups) Init(IConfiguration config)
        {
            var siteProduction = new ApiToken(config, "site-production", ResourceType.Apps, AccessType.Key);
            var apiProduction = new ApiToken(config, "api-production", ResourceType.Apps, AccessType.Key);

            return (Guid.Parse("292F6FBC-6C21-40AF-AF47-D40AC8ABB8CF"),
                    new Dictionary<string, DashboardItem[]>
                    {
                        {
                            "P2P Frontend",
                            new[]
                            {
                                DashboardItem.AddCustomEventsWhere(
                                    siteProduction,
                                    "No availability in cabin selection (OLD BOOKING)",
                                    whereQuery: @"| where * contains 'Old selection page: Missing cabin availability'",
                                    options: options => { options.Duration = ItemDuration.OneDay; }
                                ),
                                DashboardItem.AddCustomEventsWhere(
                                    siteProduction,
                                    "No availability in cabin selection (NEW BOOKING)",
                                    whereQuery: @"| where * contains 'New selection page: Missing cabin availability'",
                                    options: options => { options.Duration = ItemDuration.OneDay; }
                                )
                            }
                        },
                        {
                            "P2P Backend",
                            new[]
                            {
                                DashboardItem.AddExceptionsWhere(
                                    siteProduction,
                                    "Selection not available after all",
                                    whereQuery: @" | where operation_Name contains 'portToPort'
                                                   | where outerMessage contains 'Ship CATEGORY selected available for waitlist only'
                                                   | where outerMessage contains 'The booking is waitlisted for one or more Selling limit'",
                                    options: options => { options.Duration = ItemDuration.OneDay; }

                                ),
                                DashboardItem.AddExceptionsWhere(
                                    siteProduction,
                                    "Unable to locate quote",
                                    whereQuery: @" | where operation_Name contains 'portToPort'
                                                   | where outerMessage contains 'Unable to locate an item of type Quote in the cache with a key of'",
                                    options: options => { options.Duration = ItemDuration.OneDay; }
                                ),
                                DashboardItem.AddExceptionsWhere(
                                    siteProduction,
                                    "Exceptions",
                                    whereQuery: @" | where operation_Name contains 'portToPort'
                                                   | where outerMessage !contains 'Ship CATEGORY selected available for waitlist only'
                                                   | where outerMessage !contains 'The booking is waitlisted for one or more Selling limit'
                                                   | where outerMessage !contains 'Unable to locate an item of type Quote in the cache with a key of'
                                                   | where type !contains 'MissingCabinViewValueOnPolarOutsideCabin'
                                                   | where type !contains 'MissingCabinDescriptionException'
                                                   | where type !contains 'MissingMealDescriptionException'
                                                   | where customDimensions !contains 'VoyageCacheDictionary'
                                                   | where customDimensions !contains 'SearchVoyageKeyDictionary'"
                                ),

                                DashboardItem.AddExceptionsWhere(
                                    siteProduction,
                                    "Missing meal description",
                                    whereQuery: @"| where type contains 'MissingMealDescriptionException'",
                                    options: options =>
                                    {
                                        options.ErrorThreshold = 1;
                                        options.Duration = ItemDuration.OneDay;
                                    }
                                ),

                                DashboardItem.AddExceptionsWhere(
                                    siteProduction,
                                    "Missing cabin description",
                                    whereQuery: @"| where type contains 'MissingCabinDescriptionException'",
                                    options: options =>
                                    {
                                        options.ErrorThreshold = 1;
                                        options.Duration = ItemDuration.OneDay;
                                    }
                                ),

                                DashboardItem.AddExceptionsWhere(
                                    siteProduction,
                                    "Missing view attribute on Polar Outside cabin",
                                    whereQuery: @"| where type contains 'MissingCabinViewValueOnPolarOutsideCabin'",
                                    options: options =>
                                    {
                                        options.ErrorThreshold = 1;
                                        options.Duration = ItemDuration.OneDay;
                                    }
                                )
                            }
                        },
                        {
                            "Gateway",
                            new[]
                            {
                                DashboardItem.AddExceptionsWhere(
                                    apiProduction,
                                    "SeaWare data loading issue",
                                    whereQuery: @"| where * contains 'Data Loading issue'",
                                    options: options => { options.ErrorThreshold = 1; }
                                ),
                                DashboardItem.AddExceptionsWhere(
                                    apiProduction,
                                    "Exceptions from PG",
                                    whereQuery: @"| where operation_Name contains 'api/pg'"
                                ),
                                DashboardItem.AddExceptionsWhere(
                                    apiProduction,
                                    "Availability",
                                    whereQuery: @"| where operation_Name startswith 'POST /api/portToPortAvailability'"),

                                DashboardItem.AddExceptionsWhere(
                                    apiProduction,
                                    "Dbms lock",
                                    whereQuery: @"| where customDimensions  contains 'UpdateBooking: DbmsLock'",
                                    options: options => { options.Duration = ItemDuration.OneDay; }
                                ),

                                DashboardItem.AddExceptionsWhere(
                                    apiProduction,
                                    "Voyage cache dictionary",
                                    whereQuery: @"| where customDimensions contains 'VoyageCacheDictionary'",
                                    options: options => { options.Duration = ItemDuration.OneDay; }
                                ),

                                DashboardItem.AddExceptionsWhere(
                                    apiProduction,
                                    "Search voyage key dictionary",
                                    whereQuery: @"| where customDimensions contains 'SearchVoyageKeyDictionary'",
                                    options: options => { options.Duration = ItemDuration.OneDay; }
                                )
                            }
                        }
                    }
                );
        }
    }
}