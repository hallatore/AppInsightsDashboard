using System;
using Microsoft.Extensions.Configuration;

namespace ConfigLogic
{
    public class ApiToken
    {
        public ApiToken(IConfiguration config, string name)
        {
            Id = Guid.Parse(config[$"tokens:{name}:id"]);
            Key = config[$"tokens:{name}:key"];
        }

        public Guid Id { get; }
        public string Key { get; }
    }
}