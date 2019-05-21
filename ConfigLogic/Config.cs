using System;
using System.Collections.Generic;
using System.Linq;
using ConfigLogic.Dashboard;
using Microsoft.Extensions.Configuration;

namespace ConfigLogic
{
    public class Config
    {
        public IReadOnlyDictionary<Guid, Dictionary<string, DashboardItem[]>> Dashboards { get; }

        public Config(IConfiguration configuration)
        {
            var dashboards = new Dictionary<Guid, Dictionary<string, DashboardItem[]>>();
            var entryAssembly = System.Reflection.Assembly.GetEntryAssembly();

            foreach (var typeInfo in entryAssembly.DefinedTypes)
            {
                if (!typeInfo.ImplementedInterfaces.Contains(typeof(IConfig)))
                {
                    continue;
                }

                var config = (IConfig)entryAssembly.CreateInstance(typeInfo.FullName);
                var (id, groups) = config.Init(configuration);
                dashboards.Add(id, groups);
            }

            Dashboards = dashboards;
        }
    }
}
