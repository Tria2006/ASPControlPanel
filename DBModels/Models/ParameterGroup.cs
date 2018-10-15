using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBModels.Models
{
    public class ParameterGroup : BaseModel
    {
        [DisplayName("Может повторяться")]
        public bool CanRepeat { get; set; }

        [DisplayName("Глобальная")]
        public bool IsGlobal { get; set; }
    }
}
