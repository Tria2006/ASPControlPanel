﻿
using System.ComponentModel.DataAnnotations;

namespace IGSLControlPanel.Enums
{
    public enum DataTypes
    {
        [Display(Name = "Не выбрано")]
        empty = 0,
        [Display(Name = "Число")]
        Number = 1,
        [Display(Name = "Дата")]
        Date = 2,
        [Display(Name = "Список")]
        List = 3,
        [Display(Name = "Строка")]
        String = 4
    }
}
