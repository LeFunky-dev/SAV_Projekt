using GalaSoft.MvvmLight;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using SAV_Projekt.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows.Media;

namespace SAV_Projekt.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private const string targetDirectory = @"..\..\..\ETF_Data\";
        public ObservableCollection<Etf> ETFs { get; set; }
        public Func<double, string> Formatter { get; set; }
        public SeriesCollection Series { get; set; }
        public ChartValues<EtfValue> Values { get; set; }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ETFs = new ObservableCollection<Etf>();
            Values = new ChartValues<EtfValue>();
            var dayConfig = Mappers.Xy<EtfValue>()
                .X(dayModel => dayModel.Date.ToOADate())
                .Y(dayModel => dayModel.Value);
            Series = new SeriesCollection(dayConfig);

            InitValues();


            Formatter = value => new System.DateTime((long)(value * TimeSpan.FromDays(30).Ticks)).ToString("t");
        }

        private void InitValues()
        {
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
            {
                using (var reader = new StreamReader(fileName))
                {
                    var line = reader.ReadLine();
                    Etf ETF = new Etf()
                    {
                        Name = line.Split(',')[1],
                        Values = new ChartValues<EtfValue>()
                    };
                    while (!reader.EndOfStream)
                    {
                        line = reader.ReadLine();
                        var values = line.Split(',');

                        ETF.Values.Add(new EtfValue()
                        {
                            Date = DateTime.ParseExact(values[0], "yyyy-MM", CultureInfo.InvariantCulture),
                            Value = double.Parse(values[1], new CultureInfo("en-US"))
                        });
                        
                    }
                    Series.Add(new LineSeries()
                    {
                        Values = ETF.Values,
                        Fill = Brushes.Transparent
                    });
                    ETFs.Add(ETF);
                    
                }
                break;
            }
        }
    }
}
