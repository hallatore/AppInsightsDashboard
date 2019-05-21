using System;
using System.Collections.Generic;
using ConfigLogic.Dashboard;
using Microsoft.Extensions.Configuration;

namespace ConfigLogic
{
    public interface IConfig
    {
        (Guid id, Dictionary<string, DashboardItem[]> groups) Init(IConfiguration config);
    }
}