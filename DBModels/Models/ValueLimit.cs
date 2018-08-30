using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DBModels.Models
{
    public class ValueLimit : BaseModel
    {
        [DisplayName("Тип данных")]
        public int ParameterDataType { get; set; }

        [DisplayName("Строковое значение")]
        public string StringValue { get; set; }

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

        [DisplayName("Список значений")]
        public List<LimitListItem> LimitListItems { get; set; } = new List<LimitListItem>();

        public Guid ParameterId { get; set; }

        public Guid ProductId { get; set; }
    }
}
