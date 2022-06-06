using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAV_Projekt.Model
{
    public class Portfolio
    {
        public string Name { get; set; }
        public ObservableCollection<PortfolioEtf> PortfolioEtfs { get; set; }

    }
}
