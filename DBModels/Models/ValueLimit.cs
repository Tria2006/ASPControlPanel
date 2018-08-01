using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DBModels.Models
{
    public class ValueLimit : BaseModel
    {
        [DisplayName("Тип данных")]
        public int ParameterDataType { get; set; }

        [DisplayName("Начальное значение")]
        public int? IntValueFrom { get; set; }

        [DisplayName("Конечное значение")]
        public int? IntValueTo { get; set; }

        [DisplayName("Начальная дата")]
        [DataType(DataType.Date)]
        public DateTime? DateValueFrom { get; set; }

        [DisplayName("Конечная дата")]
        [DataType(DataType.Date)]
        public DateTime? DateValueTo { get; set; }

        public Guid ParameterId { get; set; }

        public Guid ProductId { get; set; }
    }
}
