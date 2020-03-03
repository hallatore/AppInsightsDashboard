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
                                    whereQuery: @"| where * contains 'Old selection page: Missing cabin availability'"
                                ),
                                DashboardItem.AddCustomEventsWhere(
                                    siteProduction,
                                    "No availability in cabin selection (NEW BOOKING)",
                                    whereQuery: @"| where * contains 'New selection page: Missing cabin availability'"
                                ),
                            }
                        },
                        {
                            "P2P Backend",
                            new[]
                            {
                                DashboardItem.AddExceptionsWhere(
                                    siteProduction,
                                    "Missing cabin description",
                                    whereQuery: @"| where type contains 'MissingCabinDescriptionException'",
                                    options: options =>
                                    {
                                        options.ErrorThreshold = 1;
                                    }
                                ),
                                DashboardItem.AddExceptionsWhere(
                                    siteProduction,
                                    "Missing view attribute on Polar Outside cabin",
                                    whereQuery: @"| where type contains 'MissingCabinViewValueOnPolarOutsideCabin'",
                                    options: options =>
                                    {
                                        options.ErrorThreshold = 1;
                                    }
                                ),
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
                                    options: options =>
                                    {
                                        options.ErrorThreshold = 1;
                                    }
                                ),
                                DashboardItem.AddExceptionsWhere(
                                    apiProduction,
                                    "Exceptions from PG",
                                    whereQuery: @"| where operation_Name contains 'api/pg'"
                                ),
                            }
                        },
                    }
                );
        }
    }
}