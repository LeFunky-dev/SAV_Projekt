using LiveCharts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAV_Projekt.Model
{
    public class Etf
    {
        public string Name { get; set; }
        public ChartValues<EtfValue> Values { get; set; }
    }
}
