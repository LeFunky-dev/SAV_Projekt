using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAV_Projekt.Model
{
    public class PortfolioEtf
    {
        public Etf Etf { get; set; }
        public double PercentageOfPortfolio { get; set; }
        public ObservableCollection<Etf> AvailableEtfs { get; set; }
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
    }
}
