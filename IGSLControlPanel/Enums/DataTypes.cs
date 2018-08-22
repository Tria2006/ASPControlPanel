
using System.ComponentModel.DataAnnotations;

namespace IGSLControlPanel.Enums
{
    public enum DataTypes
    {
        [Display(Name = "")]
        empty = 0,
        [Display(Name = "Число")]
        Number = 1,
        [Display(Name = "Дата")]
        Date = 2,
        [Display(Name = "Список")]
        List = 3
    }
}
