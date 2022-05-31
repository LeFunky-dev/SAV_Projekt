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
        private Portfolio firstPortfolioToCompare;
        public Func<double, string> Formatter { get; set; }
        public Portfolio FirstPortfolioToCompare
        {
            get { return firstPortfolioToCompare; }
            set { 
                firstPortfolioToCompare = value;
                RaisePropertyChanged("FirstPortfolioToCompare");
            }
        }
        private Portfolio secondPortfolioToCompare;
        public Portfolio SecondPortfolioToCompare
        {
            get { return secondPortfolioToCompare; }
            set { 
                secondPortfolioToCompare = value;
                CalcPortfolioSeries(true);
                RaisePropertyChanged("SecondPortfolioToCompare");
            }
        }
        public SeriesCollection FirstPortfolioToDisplay { get; set; }
        public ObservableCollection<Etf> ETFs { get; set; }
        public ICommand ShowEtfDetailCommand { get { return new RelayCommand<PortfolioEtf>(ShowEtfDetail); } }

        private void ShowEtfDetail(PortfolioEtf portfolioEtf)
        {
            Messenger.Default.Send(OperatingCommandsEnum.OpenEtfDetail);
            Messenger.Default.Send(new NotificationMessage<Etf>(portfolioEtf.Etf, OperatingCommandsEnum.ShowEtfDetail.ToString()));
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ETFs = new ObservableCollection<Etf>();
            FirstPortfolioToCompare = new Portfolio();
            AllPortfolios = new ObservableCollection<Portfolio>();
            InitValues();
            InitPortfolios();
        }

        private void InitPortfolios()
        {
            Portfolio Portfolio7030= new Portfolio()
            {
                Name = "Portfolio 70/30",
                PortfolioEtfs = new ObservableCollection<PortfolioEtf>()
            };
            Portfolio7030.PortfolioEtfs.Add(new PortfolioEtf()
            {
                Etf = ETFs[0],
                PercentageOfPortfolio = 0.7
            });
            Portfolio7030.PortfolioEtfs.Add(new PortfolioEtf()
            {
                Etf = ETFs[1],
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
                Etf = ETFs[3],
                PercentageOfPortfolio = 0.05
            });
            PortfolioGlobal.PortfolioEtfs.Add(new PortfolioEtf()
            {
                Etf = ETFs[2],
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
                            Value = double.Parse(values[1], new CultureInfo("en-US"))
                        });
                        
                    }
                    ETFs.Add(ETF);
                }
            }
        }
        private void CalcPortfolioSeries(bool firstOrSecondPortfolio)
        {
            var dayConfig = Mappers.Xy<EtfValue>()
               .X(dayModel => dayModel.Date.Ticks)
               .Y(dayModel => dayModel.Value);
            Formatter = value => new DateTime((long)value).ToString("yyyy-MM");
            FirstPortfolioToDisplay = new SeriesCollection(dayConfig);
            ChartValues<EtfValue> etfValues = new ChartValues<EtfValue>();
            List<DateTime> minDates = new List<DateTime>();
            foreach ( var etf in SecondPortfolioToCompare.PortfolioEtfs)
            {
                minDates.Add(etf.Etf.Values[0].Date); 
            }
            minDates.Sort();
            minDates.Reverse();
            foreach (var etf in SecondPortfolioToCompare.PortfolioEtfs)
            {
                foreach(var value in etf.Etf.Values)
                {
                    if(value.Date >= minDates[0])
                    {
                        if(firstOrSecondPortfolio)
                        {
                            bool found = false;
                            var index = 0;
                            foreach(var storedValue in etfValues)
                            {
                                
                                if(storedValue.Date == value.Date)
                                {
                                    found = true;
                                    index = etfValues.IndexOf(storedValue);
                                }
                                
                            }
                            if(found)
                            {
                                etfValues[index].Value += value.Value * etf.PercentageOfPortfolio;
                            }
                            else
                            {
                                etfValues.Add(new EtfValue()
                                {
                                    Date = value.Date,
                                    Value = value.Value * etf.PercentageOfPortfolio
                                });
                            }
                        }
                    }
                }
            }
            FirstPortfolioToDisplay.Add(new LineSeries()
            {
                Values = etfValues,
            });
             etfValues = new ChartValues<EtfValue>();
             minDates = new List<DateTime>();
            foreach (var etf in FirstPortfolioToCompare.PortfolioEtfs)
            {
                minDates.Add(etf.Etf.Values[0].Date);
            }
            minDates.Sort();
            minDates.Reverse();
            foreach (var etf in FirstPortfolioToCompare.PortfolioEtfs)
            {
                foreach (var value in etf.Etf.Values)
                {
                    if (value.Date >= minDates[0])
                    {
                        if (firstOrSecondPortfolio)
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
                            if (found)
                            {
                                etfValues[index].Value += value.Value * etf.PercentageOfPortfolio;
                            }
                            else
                            {
                                etfValues.Add(new EtfValue()
                                {
                                    Date = value.Date,
                                    Value = value.Value * etf.PercentageOfPortfolio
                                });
                            }
                        }
                    }
                }
            }
            FirstPortfolioToDisplay.Add(new LineSeries()
            {
                Values= etfValues,
            });
            RaisePropertyChanged("FirstPortfolioToDisplay");
        }
    }
}
