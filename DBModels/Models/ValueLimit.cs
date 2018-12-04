using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DBModels.Models
{
    public class ValueLimit : BaseModel
    {
        public ValueLimit()
        {
            
        }

        public ValueLimit(ValueLimit source)
        {
            DateValueFrom = source.DateValueFrom;
            DateValueTo = source.DateValueTo;
            IntValueFrom = source.IntValueFrom;
            IntValueTo = source.IntValueTo;
            Name = source.Name;
            IsDeleted = source.IsDeleted;
            ParameterDataType = source.ParameterDataType;
            ParameterId = source.Id;
            ProductId = source.ProductId;
            StringValue = source.StringValue;
            ValidFrom = source.ValidFrom;
            ValidTo = source.ValidTo;

            if (source.LimitListItems == null || !source.LimitListItems.Any()) return;

            if(LimitListItems == null) LimitListItems = new List<LimitListItem>();
            foreach (var sourceItem in source.LimitListItems)
            {
                var item = new LimitListItem
                {
                    Name = sourceItem.Name,
                    Value = sourceItem.Value,
                    ValidFrom = DateTime.Now,
                    ValidTo = new DateTime(2100, 1, 1),
                    ValueLimitId = Id
                };
                LimitListItems.Add(item);
            }
        }

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
