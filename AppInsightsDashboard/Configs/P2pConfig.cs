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

            return (Guid.Parse("292F6FBC-6C21-40AF-AF47-D40AC8ABB8CF"),
                    new Dictionary<string, DashboardItem[]>
                    {
                        {
                            "P2P Frontend",
                            new[]
                            {
                                DashboardItem.AddCustomEventsWhere(
                                    siteProduction,
                                    "Browser events",
                                    whereQuery: @""
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
                            }
                        },
                    }
                );
        }
    }
}