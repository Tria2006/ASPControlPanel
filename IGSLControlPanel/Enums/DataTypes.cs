
using System.ComponentModel.DataAnnotations;

namespace IGSLControlPanel.Enums
{
    public enum DataTypes
    {
        [Display(Name = "")]
        empty = 0,
        //Строка = 0,
        Число = 1,
        Дата = 2,
        //Логический = 3,
        //Список = 4
    }
}
