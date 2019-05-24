using System;
using System.Collections.Generic;
using ConfigLogic.Dashboard;

namespace ConfigLogic
{
    public class Config
    {
        public IReadOnlyDictionary<Guid, Dictionary<string, DashboardItem[]>> Dashboards { get; }

        public Config(IReadOnlyDictionary<Guid, Dictionary<string, DashboardItem[]>> dashboards)
        {
            Dashboards = dashboards;
        }
    }
}
