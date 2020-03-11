using System;
using System.Collections.Generic;
using AppInsights;
using ConfigLogic;
using ConfigLogic.Dashboard;
using Microsoft.Extensions.Configuration;

namespace AppInsightsDashboard.Configs
{
    public class B2cConfig : IConfig
    {
        public (Guid id, Dictionary<string, DashboardItem[]> groups) Init(IConfiguration config)
        {
            var siteProduction = new ApiToken(config, "site-production", ResourceType.Apps, AccessType.Key);
            var apiProduction = new ApiToken(config, "api-production", ResourceType.Apps, AccessType.Key);
            var myBookingProduction = new ApiToken(config, "my-booking-production", ResourceType.Apps, AccessType.Key);
            var pgProduction = new ApiToken(config, "pg-production", ResourceType.Apps, AccessType.Key);
            var swStaging = new ApiToken(config, "staging-workspace", ResourceType.Workspaces, AccessType.AppSecret);

            return (Guid.Parse("7fd512f1-d1a0-4353-92da-50f02207d70e"),
                    new Dictionary<string, DashboardItem[]>
                    {
                        {
                            "B2C Website",
                            new[]
                            {
                                DashboardItem.AddRequestPerMinute(siteProduction),
                                DashboardItem.AddRequestResponseTime(siteProduction),
                                DashboardItem.AddFailedRequestsPercentage(siteProduction),
                                DashboardItem.AddExceptionPerMinute(siteProduction, whereQuery: "| where type != 'Hurtigruten.Web.Presentation.Controllers.JavaScriptException'"),
                                DashboardItem.AddExceptionPerMinute(
                                    siteProduction, 
                                    "JavaScript errors", 
                                    whereQuery: "| where type == 'Hurtigruten.Web.Presentation.Controllers.JavaScriptException'", 
                                    options: options =>
                                    {
                                        options.WarningThreshold = 0; 
                                        options.ErrorThreshold = 200;
                                    }),
                                DashboardItem.AddWebTestsPercentage(siteProduction),
                                DashboardItem.AddExceptionsWhere(
                                    siteProduction,
                                    "Task Canceled",
                                    whereQuery: "| where type contains 'TaskCanceledException'"),

                                DashboardItem.AddExceptionsWhere(
                                    siteProduction,
                                    "Null Reference",
                                    whereQuery: "| where type contains 'NullReferenceException'"),

                                DashboardItem.AddExceptionsWhere(
                                    siteProduction,
                                    "Misc exceptions",
                                    whereQuery: @"| where 
                                        type contains 'IndexOutOfRangeException' or 
                                        type contains 'InvalidOperationException' or 
                                        type contains 'ArgumentNullException' or 
                                        type contains 'UriFormatException'"),

                                DashboardItem.AddRequestPerMinute(
                                    siteProduction, 
                                    "Login prompts",
                                    options: options =>
                                    {
                                        options.Postfix = string.Empty;
                                        options.Total = ItemTotal.Sum;
                                        options.WarningThreshold = 100;
                                        options.ErrorThreshold = 1000;
                                    },
                                    whereQuery: @"| where url contains 'Util/login.aspx' and url !contains 'EPiServer%2fCMS' and url !contains '%2fmvc%2f' and url !contains '%2fmodules%2f'")
                            }
                        },
                        {
                            "B2C Gateway API",
                            new[]
                            {
                                DashboardItem.AddRequestPerMinute(apiProduction),
                                DashboardItem.AddRequestResponseTime(apiProduction),
                                DashboardItem.AddFailedRequestsPercentage(apiProduction),
                                DashboardItem.AddExceptionPerMinute(apiProduction),
                                DashboardItem.AddWebTestsPercentage(apiProduction),
                                DashboardItem.AddExceptionsWhere(
                                    apiProduction,
                                    "Task Canceled",
                                    whereQuery: "| where type contains 'TaskCanceledException'"),

                                DashboardItem.AddExceptionsWhere(
                                    apiProduction,
                                    "Null Reference",
                                    whereQuery: "| where type contains 'NullReferenceException'"),

                                DashboardItem.AddExceptionsWhere(
                                    apiProduction,
                                    "Misc exceptions",
                                    whereQuery: @"| where 
                                        type contains 'IndexOutOfRangeException' or 
                                        type contains 'InvalidOperationException' or 
                                        type contains 'ArgumentNullException' or 
                                        type contains 'UriFormatException'"),
                                DashboardItem.AddExceptionsWhere(
                                    apiProduction,
                                    "Availability",
                                    whereQuery: @"| where operation_Name startswith 'POST /api/availability'"),
                                DashboardItem.AddExceptionsWhere(
                                    apiProduction,
                                    "P2P Availability",
                                    whereQuery: @"| where operation_Name startswith 'POST /api/portToPortAvailability'"),

                                DashboardItem.AddExceptionsWhere(
                                    apiProduction,
                                    "Cabin selection",
                                    whereQuery: @"| where 
                                        operation_Name startswith 'POST /api/quotes/' and 
                                        operation_Name  endswith '/cabins' and
                                        customDimensions !contains 'booking is waitlisted' and
                                        customDimensions !contains 'insufficient available capacity' and
                                        customDimensions !contains 'was not found in the cached'"),
                                DashboardItem.AddFailedRequestsPercentage(
                                    apiProduction,
                                    "api/quotes 24h",
                                    item => {
                                        item.Duration = ItemDuration.OneDay;
                                        item.StatusSplitFactor = 24;
                                    },
                                    "| where name startswith 'GET /api/quotes/'"),
                                DashboardItem.AddExceptionsWhere(
                                    apiProduction,
                                    "Redis",
                                    whereQuery: "| where type contains 'Redis'"),
                                DashboardItem.AddExceptionsWhere(
                                    apiProduction,
                                    "PG/Seaware",
                                    whereQuery: @"| where
                                        customDimensions contains 'SwBizLogic/Service.svc' or
                                        customDimensions contains 'is already locked' or
                                        customDimensions contains 'communication timeout' or
                                        customDimensions contains 'Wait for another thread failed' or
                                        customDimensions contains 'Query Execution Time exceeds maximum allowed'")
                            }
                        },
                        {
                            "My booking",
                            new []
                            {
                                DashboardItem.AddRequestPerMinute(myBookingProduction),
                                DashboardItem.AddFailedRequestsPercentage(myBookingProduction),
                                DashboardItem.AddExceptionPerMinute(myBookingProduction),
                                DashboardItem.AddWebTestsPercentage(myBookingProduction)
                            }
                        },
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
                                    whereQuery: "| where operation_Name startswith 'ICapacityPricingSeaware'")
                            }
                        },
                        {
                            "Seaware",
                            new[]
                            {
                                DashboardItem.AddRawQuery(
                                    apiProduction,
                                    "Days since last issue",
                                    query: @"
                                        let exceptionsOperationIds = exceptions
                                        | where outerMessage !contains 'Redis'
                                        | where outerMessage contains 'connecting' or outerMessage contains 'connection' or outerMessage contains 'SwBizLogic/Service.svc'
                                        | where timestamp > ago(300d)
                                        | distinct operation_Id;
                                        requests
                                        | where timestamp > ago(300d)
                                        | where name startswith 'POST /api/availability'
                                        | where (operation_Id in (exceptionsOperationIds))
                                        | where datetime_part('hour', timestamp) > 9 and datetime_part('hour', timestamp) < 21
                                        | summarize errors = countif(success == false) by bin(timestamp, 1d)
                                        | where errors > 100
                                        | summarize max(timestamp), toint((now() - max(timestamp)) / 1d)"
                                )
                            }
                        }
                    }
                );
        }
    }
}