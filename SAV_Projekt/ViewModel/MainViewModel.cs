using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
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
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace SAV_Projekt.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private const string targetDirectory = @"..\..\..\ETF_Data\";
        public ObservableCollection<Portfolio> AllPortfolios { get; set; }
        public ObservableCollection<ValueGrowth> ValueGrowthFirst { get; set; }
        public ObservableCollection<ValueGrowth> ValueGrowthSecond { get; set; }
        private Portfolio firstPortfolioToCompare;
        public Func<double, string> Formatter { get; set; }
        public Portfolio FirstPortfolioToCompare
        {
            get { return firstPortfolioToCompare; }
            set
            {
                firstPortfolioToCompare = value;
                if (firstPortfolioToCompare != null && secondPortfolioToCompare != null && firstPortfolioToCompare.PortfolioEtfs != null && secondPortfolioToCompare.PortfolioEtfs != null)
                {
                    CalcPortfolioSeries();
                }

                RaisePropertyChanged("FirstPortfolioToCompare");
            }
        }
        private Portfolio secondPortfolioToCompare;
        public Portfolio SecondPortfolioToCompare
        {
            get { return secondPortfolioToCompare; }
            set
            {
                secondPortfolioToCompare = value;
                if (firstPortfolioToCompare != null && secondPortfolioToCompare != null && firstPortfolioToCompare.PortfolioEtfs != null && secondPortfolioToCompare.PortfolioEtfs != null)
                {
                    CalcPortfolioSeries();
                }
                RaisePropertyChanged("SecondPortfolioToCompare");
            }
        }
        public SeriesCollection FirstPortfolioToDisplay { get; set; }
        public ObservableCollection<Etf> ETFs { get; set; }
        public ICommand ShowEtfDetailCommand { get { return new RelayCommand<PortfolioEtf>(ShowEtfDetail); } }
        public ICommand ResetPortfolioComparisonCommand { get { return new RelayCommand(ResetPortfolioComparison); } }

        private void ResetPortfolioComparison()
        {
            List<DateTime> minDates = new List<DateTime>();
            if (ETFs != null)
            {
                foreach (var etf in SecondPortfolioToCompare.PortfolioEtfs)
                {
                    minDates.Add(etf.Etf.Values[0].Date);
                }
                foreach (var etf in FirstPortfolioToCompare.PortfolioEtfs)
                {
                    minDates.Add(etf.MinDate);
                }
                minDates.Sort();
                minDates.Reverse();
                MinDate = minDates[0];
                MaxDate = FirstPortfolioToCompare.PortfolioEtfs[0].Etf.Values[FirstPortfolioToCompare.PortfolioEtfs[0].Etf.Values.Count -1].Date;
            }
        }

        private void ShowEtfDetail(PortfolioEtf portfolioEtf)
        {
            Messenger.Default.Send(OperatingCommandsEnum.OpenEtfDetail);
            Messenger.Default.Send(new NotificationMessage<Etf>(portfolioEtf.Etf, OperatingCommandsEnum.ShowEtfDetail.ToString()));
        }
        private DateTime minDate;
        private DateTime maxDate;
        public DateTime MinDate
        {
            get { return minDate; }
            set
            {
                minDate = value;
                MinDateDouble = value.Ticks;
                RaisePropertyChanged("MinDateDouble");
                RaisePropertyChanged("MinDate");
            }
        }
        public DateTime MaxDate
        {
            get
            {
                return maxDate;
            }
            set
            {
                maxDate = value;
                MaxDateDouble = value.Ticks;
                RaisePropertyChanged("MaxDateDouble");
                RaisePropertyChanged("MaxDate");
            }
        }
        public double MinDateDouble { get; set; }
        public double MaxDateDouble { get; set; }
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ETFs = new ObservableCollection<Etf>();
            FirstPortfolioToCompare = new Portfolio()
            {
                PortfolioEtfs = new ObservableCollection<PortfolioEtf>()
            };
            SecondPortfolioToCompare = new Portfolio()
            {
                PortfolioEtfs = new ObservableCollection<PortfolioEtf>()
            };
            AllPortfolios = new ObservableCollection<Portfolio>();
            ValueGrowthFirst = new ObservableCollection<ValueGrowth>();
            ValueGrowthSecond = new ObservableCollection<ValueGrowth>();
            InitValues();
            InitPortfolios();
            InitValueGrowth(FirstPortfolioToDisplay);

        }

        private void InitValueGrowth(SeriesCollection portfolio)
        {
            for(int j = 0; j < portfolio.Count; j++)
            {
                var endDate = ((EtfValue)portfolio[j].Values[portfolio[j].Values.Count - 1]).Date;
                var endPerformance = ((EtfValue)portfolio[j].Values[portfolio[j].Values.Count - 1]).Value;
                var startDate = ((EtfValue)portfolio[j].Values[0]).Date;
                int[] vals = { 1, 4, 5, 5, 5, 10, 10, 10, 10, 10, 10, 10, 10 };
                int totalVal = 0;
                int i = 0;
                endDate = endDate.AddYears(-vals[i]);
                totalVal += vals[i];
                while (endDate > startDate)
                {
                    foreach (EtfValue entry in portfolio[j].Values)
                    {
                        if (entry.Date == endDate)
                        {
                            var calcPerf = endPerformance - entry.Value;
                            if(j<= 0)
                            {
                                ValueGrowthFirst.Add(new ValueGrowth()
                                {
                                    Period = totalVal > 1 ? totalVal + " Jahre" : totalVal + " Jahr",
                                    Performance = calcPerf.ToString("0.00") + "%",
                                    Color = calcPerf > 0 ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red),
                                });
                            }
                            else
                            {
                                ValueGrowthSecond.Add(new ValueGrowth()
                                {
                                    Period = totalVal > 1 ? totalVal + " Jahre" : totalVal + " Jahr",
                                    Performance = calcPerf.ToString("0.00") + "%",
                                    Color = calcPerf > 0 ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red),
                                });
                            }
                            
                        }
                    }
                    i++;
                    totalVal += vals[i];
                    endDate = endDate.AddYears(-vals[i]);
                }
            }
            
        }

        private void InitPortfolios()
        {
            Portfolio Portfolio7030 = new Portfolio()
            {
                Name = "Portfolio 70/30",
                PortfolioEtfs = new ObservableCollection<PortfolioEtf>()
            };
            Portfolio7030.PortfolioEtfs.Add(new PortfolioEtf()
            {
                Etf = ETFs[0],
                MinDate = ETFs[0].Values[0].Date,
                MaxDate = ETFs[0].Values[ETFs[0].Values.Count-1].Date,
                PercentageOfPortfolio = 0.7
            });
            Portfolio7030.PortfolioEtfs.Add(new PortfolioEtf()
            {
                Etf = ETFs[4],
                MinDate = ETFs[4].Values[0].Date,
                MaxDate = ETFs[4].Values[ETFs[4].Values.Count-1].Date,
                PercentageOfPortfolio = 0.3
            });
            FirstPortfolioToCompare = Portfolio7030;
            RaisePropertyChanged("FirstPortfolioToCompare");

            Portfolio PortfolioGlobal = new Portfolio()
            {
                Name = "Portfolio Global",
                PortfolioEtfs = new ObservableCollection<PortfolioEtf>()
            };
            PortfolioGlobal.PortfolioEtfs.Add(new PortfolioEtf()
            {
                Etf = ETFs[9],
                MinDate = ETFs[9].Values[0].Date,
                MaxDate = ETFs[9].Values[ETFs[9].Values.Count-1].Date,
                PercentageOfPortfolio = 0.05
            });
            PortfolioGlobal.PortfolioEtfs.Add(new PortfolioEtf()
            {
                Etf = ETFs[2],
                MinDate = ETFs[2].Values[0].Date,
                MaxDate = ETFs[2].Values[ETFs[2].Values.Count-1].Date,
                PercentageOfPortfolio = 0.95
            });
            SecondPortfolioToCompare = PortfolioGlobal;
            AllPortfolios.Add(PortfolioGlobal);
            AllPortfolios.Add(Portfolio7030);
            RaisePropertyChanged("SecondPortfolioToCompare");
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
                        Values = new ObservableCollection<EtfValue>()
                    };
                    while (!reader.EndOfStream)
                    {
                        line = reader.ReadLine();
                        var values = line.Split(',');

                        ETF.Values.Add(new EtfValue()
                        {
                            Date = DateTime.ParseExact(values[0], "yyyy-MM", CultureInfo.InvariantCulture),
                            Value = Math.Round(double.Parse(values[1], new CultureInfo("en-US")), 1)
                        });

                    }
                    ETFs.Add(ETF);
                }
            }
        }
        private void CalcPortfolioSeries()
        {
            var dayConfig = Mappers.Xy<EtfValue>()
               .X(dayModel => dayModel.Date.Ticks)
               .Y(dayModel => dayModel.Value);
            Formatter = value => new DateTime((long)value).ToString("yyyy-MM");
            FirstPortfolioToDisplay = new SeriesCollection(dayConfig);
            ChartValues<EtfValue> etfValues = new ChartValues<EtfValue>();
            List<DateTime> minDates = new List<DateTime>();
            foreach (var etf in SecondPortfolioToCompare.PortfolioEtfs)
            {
                minDates.Add(etf.Etf.Values[0].Date);
            }
            foreach (var etf in FirstPortfolioToCompare.PortfolioEtfs)
            {
                minDates.Add(etf.Etf.Values[0].Date);
            }
            minDates.Sort();
            minDates.Reverse();
            foreach (var etf in SecondPortfolioToCompare.PortfolioEtfs)
            {
                bool firstTime = false;
                double firstVal = 0;
                foreach (var value in etf.Etf.Values)
                {
                    if (value.Date >= minDates[0])
                    {

                        bool found = false;
                        var index = 0;
                        foreach (var storedValue in etfValues)
                        {

                            if (storedValue.Date == value.Date)
                            {
                                found = true;
                                index = etfValues.IndexOf(storedValue);
                            }

                        }
                        if (!firstTime)
                        {
                            firstVal = value.Value;
                            firstTime = true;
                        }
                        if (found && firstVal != 0)
                        {
                            etfValues[index].Value += (value.Value / firstVal) * 100 * etf.PercentageOfPortfolio;
                        }
                        else
                        {
                            if(firstVal != 0)
                            {
                                etfValues.Add(new EtfValue()
                                {
                                    Date = value.Date,
                                    Value = (value.Value / firstVal) * 100 * etf.PercentageOfPortfolio
                                });
                            }
                        }
                    }
                }
            }
            FirstPortfolioToDisplay.Add(new LineSeries()
            {
                Values = etfValues,
                Title = SecondPortfolioToCompare.Name,
            });
            if (minDates.Count > 0)
            {
                MinDate = minDates[0];
                MaxDate = ETFs[0].Values[ETFs[0].Values.Count - 1].Date;
            }

            etfValues = new ChartValues<EtfValue>();
            foreach (var etf in FirstPortfolioToCompare.PortfolioEtfs)
            {
                bool firstTime = false;
                double firstVal = 0;
                foreach (var value in etf.Etf.Values)
                {
                    if (value.Date >= minDates[0])
                    {

                        bool found = false;
                        var index = 0;
                        foreach (var storedValue in etfValues)
                        {

                            if (storedValue.Date == value.Date)
                            {
                                found = true;
                                index = etfValues.IndexOf(storedValue);
                            }

                        }
                        if (!firstTime)
                        {
                            firstVal = value.Value;
                            firstTime = true;
                        }
                        if (found && firstVal != 0)
                        {
                            etfValues[index].Value += (value.Value / firstVal) * 100 * etf.PercentageOfPortfolio;
                        }
                        else
                        {
                            if(firstVal != 0)
                            {
                                etfValues.Add(new EtfValue()
                                {
                                    Date = value.Date,
                                    Value = (value.Value / firstVal) * 100 * etf.PercentageOfPortfolio
                                });
                            }
                           
                        }
                    }
                }
            }
            FirstPortfolioToDisplay.Add(new LineSeries()
            {
                Values = etfValues,
                Title = FirstPortfolioToCompare.Name,
            });
            if (minDates.Count > 0)
            {
                MinDate = minDates[0] < MinDate ? minDates[0] : MinDate;
                MaxDate = ETFs[0].Values[ETFs[0].Values.Count - 1].Date;
            }
            RaisePropertyChanged("FirstPortfolioToDisplay");
        }
    }
}
