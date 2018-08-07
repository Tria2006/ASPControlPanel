using System.ComponentModel.DataAnnotations;

namespace IGSLControlPanel.Enums
{
    public enum BaseTariffTypes
    {
        [Display(Name = "Сумма в руб.")]
        Amount = 0,

        [Display(Name = "Процент от страховой суммы")]
        Percent = 1
    }
}
