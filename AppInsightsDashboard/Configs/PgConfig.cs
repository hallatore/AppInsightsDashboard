using System;
using System.Collections.Generic;
using AppInsights;
using ConfigLogic;
using ConfigLogic.Dashboard;
using Microsoft.Extensions.Configuration;

namespace AppInsightsDashboard.Configs
{
    public class PgConfig : IConfig
    {
        public (Guid id, Dictionary<string, DashboardItem[]> groups) Init(IConfiguration config)
        {
            var pgProduction = new ApiToken(config, "pg-production", ResourceType.Apps, AccessType.Key);

            return (Guid.Parse("85e66758-22f1-428e-a5e3-4ae1f16227ff"),
                    new Dictionary<string, DashboardItem[]>
                    {
                        {
                            "PG Prod",
                            new[]
                            {
                                DashboardItem.AddRequestPerMinute(pgProduction),
                                DashboardItem.AddRequestResponseTime(pgProduction),
                                DashboardItem.AddFailedRequestsPercentage(pgProduction),
                                DashboardItem.AddExceptionPerMinute(pgProduction),
                                DashboardItem.AddFailedRequestsPercentage(
                                    pgProduction,
                                    name: "Seaware errors",
                                    whereQuery: "| where operation_Name startswith 'ICapacityPricingSeaware'"),
                            }
                        }
                    }
                );
        }
    }
}