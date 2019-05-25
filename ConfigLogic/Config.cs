using System;
using System.Collections.Generic;
using ConfigLogic.Dashboard;

namespace ConfigLogic
{
    public class Config
    {
        public Config(IReadOnlyDictionary<Guid, Dictionary<string, DashboardItem[]>> dashboards)
        {
            Dashboards = dashboards;
        }

        public IReadOnlyDictionary<Guid, Dictionary<string, DashboardItem[]>> Dashboards { get; }
    }
}