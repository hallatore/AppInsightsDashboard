# Configuration

## tokens.json
Create a `tokens.json` file with the following contents

```
{
  "tokens": {
    "appInsights-production": {
      "id": "`<appId>",
      "key": "<apiKey>"
    }
  }
}
```

## AppInsightsConfig.cs
Create a config file for your dashboard. You can have multiple dashboards.
You can easily create custom cells to filter on specific things

The url for your dashboard will be `https://myDashboard/<unique guid>`

```
using System;
using System.Collections.Generic;
using ConfigLogic;
using ConfigLogic.Dashboard;
using Microsoft.Extensions.Configuration;

namespace AppInsightsDashboard.Configs
{
    public class AppInsightsConfig : IConfig
    {
        public (Guid id, Dictionary<string, DashboardItem[]> groups) Init(IConfiguration config)
        {
            var apiToken = new ApiToken(config, "appInsights-production");

            return (Guid.Parse("<unique guid>"),
                new Dictionary<string, DashboardItem[]>
                {
                    {
                        "PG Staging",
                        new []
                        {
                            DashboardItem.AddRequestPerMinute(options: item => { item.ApiToken = apiToken; }),
                            DashboardItem.AddRequestResponseTime(options: item => { item.ApiToken = apiToken; }),
                            DashboardItem.AddFailedRequestsPercentage(options: item => { item.ApiToken = apiToken; }),
                            DashboardItem.AddExceptionPerMinute(options: item => { item.ApiToken = apiToken; }),
                            DashboardItem.AddWebTestsPercentage(options: item => { item.ApiToken = apiToken; })
                        }
                    },
                    {
                        "Exceptions",
                        new []
                        {
                            DashboardItem.AddExceptionsWhere(
                                "Task Canceled",
                                item => { item.ApiToken = apiToken; },
                                "| where type contains 'TaskCanceledException'"),

                            DashboardItem.AddExceptionsWhere(
                                "Null Reference",
                                item => { item.ApiToken = apiToken; },
                                "| where type contains 'NullReferenceException'"),

                            DashboardItem.AddExceptionsWhere(
                                "Argument Null",
                                item => { item.ApiToken = apiToken; },
                                "| where type contains 'ArgumentNullException'")
                        }
                    }
                }
            );
        }
    }
}

```