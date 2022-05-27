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
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ETFs = new ObservableCollection<Etf>();
            InitValues();
            CartesianMapper<EtfValue> mapper = Mappers.Xy<EtfValue>()
              .X((item) => (double)item.Date.Ticks / TimeSpan.FromMinutes(5).Ticks) // Set interval to 5 minutes
              .Y(item => item.Value)
              .Fill((item) => item.Value > 99 ? Brushes.Red : Brushes.Blue);

            var series = new ColumnSeries()
            {
                Title = "Timestamp Values",
                Configuration = mapper,
                Values = new ChartValues<EtfValue>
                  {
                    new EtfValue() {Date = DateTime.Now, Value = 100},
                    new EtfValue() {Date = DateTime.Now.AddMinutes(15), Value = 78},
                    new EtfValue() {Date = DateTime.Now.AddMinutes(30), Value = 21}
                  }
            };
            this.SeriesCollection = new SeriesCollection() { series };
        }

        public SeriesCollection SeriesCollection { get; set; }

        public Func<double, string> LabelFormatter =>
          value => new DateTime((long)value * TimeSpan.FromMinutes(5).Ticks).ToString("t");
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
                        Values = new ObservableCollection<EtfValue>()
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
                    ETFs.Add(ETF);
                }
            }
        }
    }

}
