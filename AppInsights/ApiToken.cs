using System;

namespace AppInsights
{
    public class ApiToken
    {
        public ApiToken(dynamic config, string name, ResourceType type, AccessType accessType)
        {
            Type = type;
            Id = Guid.Parse(config[$"tokens:{name}:id"]);

            switch (accessType)
            {
                case AccessType.Key:
                    Key = config[$"tokens:{name}:key"];
                    break;
                case AccessType.AppSecret:
                    Tenant = Guid.Parse(config[$"tokens:{name}:Tenant"]);
                    ClientId = Guid.Parse(config[$"tokens:{name}:ClientId"]);
                    ClientSecret = config[$"tokens:{name}:ClientSecret"];
                    break;
            }
        }

        public Guid Id { get; }
        public string Key { get; }
        public Guid Tenant { get; }
        public Guid ClientId { get; }
        public string ClientSecret { get; }
        public ResourceType Type { get; }
    }

    public enum ResourceType
    {
        Apps,
        Workspaces
    }

    public enum AccessType
    {
        Key,
        AppSecret
    }
}