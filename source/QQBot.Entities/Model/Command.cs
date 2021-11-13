using QQBot.Entities.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace QQBot.Entities.Config
{
    [Table("t_Command")]
    public class Command : BaseModel
    {
        public string Key { get; set; }

        public string Message { get; set; }
    }
}
