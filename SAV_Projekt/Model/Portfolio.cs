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
        public ObservableCollection<PortfolioEtf> PortfolioEtfs { get; set; }
        public string Name { get; set; }
        //Todo performance in percent, kennzahlen etc
    }
}
