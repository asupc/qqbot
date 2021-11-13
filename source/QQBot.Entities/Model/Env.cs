using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace QQBot.Entities.Model
{
    [Table("t_env")]
    public class Env : BaseModel
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public bool Enable { get; set; }

        public string Remark { get; set; }

        [Write(false)]
        public IEnumerable<QLEnv> QLEnvs { get; set; }
    }

    [Table("t_ql_env")]
    public class QLEnv : BaseModel
    {
        public string QLId { get; set; }

        public string EnvId { get; set; }

    }
}