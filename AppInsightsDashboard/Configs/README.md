# Configuration

## Application ID and API key
To find your App ID and create an API key for your AppInsights application, follow [this guide](https://dev.applicationinsights.io/documentation/Authorization/API-key-and-App-ID).

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
                            DashboardItem.AddRequestPerMinute(apiToken),
                            DashboardItem.AddRequestResponseTime(apiToken),
                            DashboardItem.AddFailedRequestsPercentage(apiToken),
                            DashboardItem.AddExceptionPerMinute(apiToken),
                            DashboardItem.AddWebTestsPercentage(apiToken)
                        }
                    },
                    {
                        "Exceptions",
                        new []
                        {
                            DashboardItem.AddExceptionsWhere(apiToken,
                                "Task Canceled",
                                null,
                                "| where type contains 'TaskCanceledException'"),

                            DashboardItem.AddExceptionsWhere(apiToken,
                                "Null Reference",
                                null,
                                "| where type contains 'NullReferenceException'"),

                            DashboardItem.AddExceptionsWhere(apiToken,
                                "Argument Null",
                                null,
                                "| where type contains 'ArgumentNullException'")
                        }
                    }
                }
            );
        }
    }
}

```
