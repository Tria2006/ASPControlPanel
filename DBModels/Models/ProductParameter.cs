using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DBModels.Models.ManyToManyLinks;

namespace DBModels.Models
{
    public class ProductParameter : BaseModel
    {
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

        [DisplayName("Порядковый номер")]
        public int Order { get; set; }

        public ValueLimit Limit { get; set; }

        [DisplayName("Группа параметров")]
        public Guid? GroupId { get; set; }
    }
}
