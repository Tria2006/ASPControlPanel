using System;

namespace DBModels.Models
{
    public class LimitListItem : BaseModel
    {
        public string Value { get; set; }

        public Guid ValueLimitId { get; set; }
    }
}
