using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IGSLControlPanel.Data;
using IGSLControlPanel.Models;

namespace IGSLControlPanel.Helpers
{
    public class TariffsHelper
    {
        public Tariff CurrentTariff { get; set; }

        public bool IsCreateInProgress { get; set; }

        public void Initialize(IGSLContext _context)
        {
            
        }
    }
}
