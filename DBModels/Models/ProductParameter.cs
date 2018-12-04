using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DBModels.Models.ManyToManyLinks;

namespace DBModels.Models
{
    public class ProductParameter : BaseModel
    {
        public ProductParameter()
        {
            
        }

        public ProductParameter(ProductParameter source)
        {
            DataType = source.DataType;
            ConstantValueDate = source.ConstantValueDate;
            ConstantValueInt = source.ConstantValueInt;
            ConstantValueStr = source.ConstantValueStr;
            DataType = source.DataType;
            IsConstant = source.IsConstant;
            GroupId = source.GroupId;
            BoolValue = source.BoolValue;
            IsRequiredForCalc = source.IsRequiredForCalc;
            IsRequiredForSave = source.IsRequiredForSave;
            if(source.Limit != null) Limit = new ValueLimit(source.Limit);
            Order = source.Order;
            Name = source.Name;
            ValidFrom = DateTime.Now;
            ValidTo = new DateTime(2100,1,1);
        }

        [Range(1, int.MaxValue)]
        [DisplayName("Тип данных")]
        public int DataType { get; set; }

        public List<ProductLinkToProductParameter> LinkToProduct { get; set; } = new List<ProductLinkToProductParameter>();

        [DisplayName("Обязателен для расчета")]
        public bool IsRequiredForCalc { get; set; }

        [DisplayName("Обязателен при сохранении")]
        public bool IsRequiredForSave { get; set; }

        [DisplayName("Неизменяемый")]
        public bool IsConstant { get; set; }

        [DisplayName("Значение параметра")]
        public string ConstantValueStr { get; set; }

        [DisplayName("Значение параметра")]
        public int? ConstantValueInt { get; set; }

        [DisplayName("Значение параметра")]
        public DateTime? ConstantValueDate { get; set; }

        [DisplayName("Значение параметра")]
        public bool BoolValue { get; set; }

        [DisplayName("Порядковый номер")]
        public int Order { get; set; }

        public ValueLimit Limit { get; set; }

        [DisplayName("Группа параметров")]
        public Guid? GroupId { get; set; }

        public bool IsParamTemplate { get; set; }
    }
}
