using DBModels.Models;

namespace IGSLControlPanel.Helpers
{
    public class EntityStateHelper
    {
        public bool IsProductCreateInProgress { get; set; }
        public bool IsParameterCreateInProgress { get; set; }
        public bool IsTariffCreateInProgress { get; set; }
        public bool IsInsRuleCreateInProgress { get; set; }
        public bool IsRiskCreateInProgress { get; set; }
        public bool IsValueLimitCreateInProgress { get; set; }

        public ValueLimit LimitWOChanges { get; set; }
    }
}
