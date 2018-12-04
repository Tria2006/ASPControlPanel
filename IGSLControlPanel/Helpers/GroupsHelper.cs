using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DBModels.Models;
using IGSLControlPanel.Data;
using Microsoft.EntityFrameworkCore;

namespace IGSLControlPanel.Helpers
{
    public class GroupsHelper
    {
        public ParameterGroup SelectedGroup { get; set; }

        public async Task SelectGroup(Guid groupId, IGSLContext context)
        {
            SelectedGroup = await context.ParameterGroups.FindAsync(groupId);
        }

        public List<ProductParameter> GetSelectedGroupParameters(IGSLContext context)
        {
            if(SelectedGroup == null) return  new List<ProductParameter>();
            return context.ProductParameters
                .Include(x => x.LinkToProduct)
                .ThenInclude(x => x.Product)
                .Include(x => x.Limit)
                .ThenInclude(x => x.LimitListItems)
                .Where(x => x.GroupId == SelectedGroup.Id).ToList();
        }
    }
}
