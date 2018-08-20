using System.ComponentModel;

namespace DBModels.Models
{
    public class ParameterGroup : BaseModel
    {
        [DisplayName("Может повторяться")]
        public bool CanRepeat { get; set; }
    }
}
