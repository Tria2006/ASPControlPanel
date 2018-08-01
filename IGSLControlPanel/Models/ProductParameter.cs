using System;
using System.Collections.Generic;
using System.ComponentModel;
using IGSLControlPanel.Models.ManyToManyLinks;

namespace IGSLControlPanel.Models
{
    public class ProductParameter : BaseModel
    {
        [DisplayName("Тип данных")]
        public int DataType { get; set; }

        public List<ProductLinkToProductParameter> LinkToProduct { get; set; }

        [DisplayName("Обязателен для расчета")]
        public bool IsRequiredForCalc { get; set; }

        [DisplayName("Обязателен при сохранении")]
        public bool IsRequiredForSave { get; set; }

        [DisplayName("Порядковый номер")]
        public int Order { get; set; }

        public ValueLimit Limit { get; set; }

        [DisplayName("Группа параметров")]
        public Guid? GroupId { get; set; }
    }
}
