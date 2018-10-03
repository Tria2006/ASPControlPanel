using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DBModels.Models;
using IGSLControlPanel.Data;

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
            return context.ProductParameters.Where(x => x.GroupId == SelectedGroup.Id).ToList();
        }
    }
}
