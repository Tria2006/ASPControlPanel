using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DBModels.Models;
using IGSLControlPanel.Data;

namespace IGSLControlPanel.Helpers
{
    public class ProductParamRiskFactorLinkHelper
    {
        private List<ParameterToFactorLink> ParameterToFactorList { get; } = new List<ParameterToFactorLink>();

        public void AddParamToFactorLink(Guid productId, Guid tariffId, Guid paramId, Guid factorId)
        {
            if (!ParameterToFactorList.Any(x =>
                x.ProductId == productId && x.TariffId == tariffId && x.ProductParameterId == paramId &&
                x.RiskFactorId == factorId))
            {
                ParameterToFactorList.Add(new ParameterToFactorLink
                {
                    ProductId = productId,
                    TariffId = tariffId,
                    ProductParameterId = paramId,
                    RiskFactorId = factorId
                });
            }
        }

        public async Task SaveLinks(IGSLContext context)
        {
            if(ParameterToFactorList.Count <= 0) return;
            await context.AddRangeAsync(ParameterToFactorList);
            await context.SaveChangesAsync();
        }

        public int GetParameterToFactorListCount()
        {
            return ParameterToFactorList.Count;
        }
    }
}
