using System;

namespace QQBot.Entities.Model
{
    public class BaseModel
    {
        [Dapper.Contrib.Extensions.ExplicitKey]
        public string Id { get; set; } = Guid.NewGuid().ToString().Replace("-", "");
    }
}
