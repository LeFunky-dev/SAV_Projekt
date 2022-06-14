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
using System.Collections.Specialized;
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
        public ObservableCollection<EtfValue> Transactions { get; set; }
        public SectionsCollection SectionCollection { get; set; }
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
                if (firstPortfolioToCompare != null && secondPortfolioToCompare != null 
                    && firstPortfolioToCompare.PortfolioEtfs != null && secondPortfolioToCompare.PortfolioEtfs != null)
                {
                    CalcPortfolioSeries();
                    if(Transactions != null)
                    {
                        CalcMetrics(FirstPortfolioToCompare, true);
                    }
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
                if (firstPortfolioToCompare != null && secondPortfolioToCompare != null 
                    && firstPortfolioToCompare.PortfolioEtfs != null && secondPortfolioToCompare.PortfolioEtfs != null)
                {
                    CalcPortfolioSeries();
                    if (Transactions != null)
                    {
                        CalcMetrics(SecondPortfolioToCompare, false);
                    }
                }
                RaisePropertyChanged("SecondPortfolioToCompare");
            }
        }
        public SeriesCollection FirstPortfolioToDisplay { get; set; }
        public ObservableCollection<Etf> ETFs { get; set; }
        public ICommand ShowEtfDetailCommand { get { return new RelayCommand<PortfolioEtf>(ShowEtfDetail); } }
        public ICommand ResetPortfolioComparisonCommand { get { return new RelayCommand(ResetPortfolioComparison); } }
        public ICommand CreatePortfolioCommand { get { return new RelayCommand(CreatePortfolio); } }
        public ICommand EditFirstPortfolioCommand { get { return new RelayCommand(EditFirstPortfolio); } }
        public ICommand EditSecondPortfolioCommand { get { return new RelayCommand(EditSecondPortfolio); } }
        public ICommand ModifyTransactionsCommand { get { return new RelayCommand(ModifyTransaction); } }
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
            Transactions = new ObservableCollection<EtfValue>();
            SectionCollection = new SectionsCollection();
            //Register to collection changed event
            Transactions.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(TransactionChanged);
            //Register to messenger
            Messenger.Default.Register<NotificationMessage<Portfolio>>(this, (c) => NotificationMessageReceived(c.Notification, c.Content));
            //Reads in values from the given csv files
            InitValues();
            //Initializes two default portfolios
            InitPortfolios();
            //Calculates initial valuegrowth
            InitValueGrowth(FirstPortfolioToDisplay);

        }
        /// <summary>
        /// Method to handle the change of the transaction observable collection
        /// and calculate the metrics new
        /// </summary>
        /// <param name="sender">sender param</param>
        /// <param name="e">event args</param>
        private void TransactionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CalcMetrics(FirstPortfolioToCompare,true);
            CalcMetrics(SecondPortfolioToCompare, false);
        }
        /// <summary>
        /// Method to calculate the metrics of a portfolio
        /// </summary>
        /// <param name="portfolio">Portfolio to calc metrics for</param>
        /// <param name="isFirst">Determines whether first or second portfolio should be calculated</param>
        private void CalcMetrics(Portfolio portfolio, bool isFirst)
        {
            double startVal = 0;
            double investments = 0.0;
            //Iterate over every transaction
            foreach (var transaction in Transactions)
            {
                if (transaction != null)
                {
                    investments += transaction.Value;
                    //Iterate over every etf
                    foreach (var entry in portfolio.PortfolioEtfs)
                    {
                        bool found = false;
                        double startingPercent = 0;
                        double absoluteVal = transaction.Value * entry.PercentageOfPortfolio;
                        //Add up every value to a total
                        foreach (var value in entry.Etf.Values)
                        {
                            if (value.Date == transaction.Date && !found)
                            {
                                startingPercent = value.Value;
                                found = true;
                            }
                            if (found && value == entry.Etf.Values[entry.Etf.Values.Count - 1])
                            {
                                absoluteVal *= (value.Value / startingPercent);
                            }
                        }
                        startVal += absoluteVal;
                    }
                }
            }
            //Sets the specs for the portfolio
            portfolio.PriceGain = Math.Round(startVal,2,MidpointRounding.ToEven);
            portfolio.Profit = Math.Round(startVal - investments,2,MidpointRounding.ToEven);
            portfolio.Investments = Math.Round(investments/1.0,2,MidpointRounding.ToEven);
            if(isFirst)
            {
                RaisePropertyChanged("FirstPortfolioToCompare");
            }
            else
            {
                RaisePropertyChanged("SecondPortfolioToCompare");
            }
        }
        /// <summary>
        /// Method to handle the ModifyTransactionCommand
        /// </summary>
        private void ModifyTransaction()
        {
            ObservableCollection<DateTime> dates = new ObservableCollection<DateTime>();
            dates.Add(minDate);
            dates.Add(maxDate);
            Messenger.Default.Send(OperatingCommandsEnum.OpenAddTransaction);
            Messenger.Default.Send(new NotificationMessage<ObservableCollection<EtfValue>>(Transactions, OperatingCommandsEnum.OpenAddTransaction.ToString()));
            Messenger.Default.Send(new NotificationMessage<ObservableCollection<DateTime>>(dates, OperatingCommandsEnum.OpenAddTransaction.ToString()));
        }
        /// <summary>
        /// Method to reset the comparison to the default date
        /// </summary>
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
                MaxDate = FirstPortfolioToCompare.PortfolioEtfs[0].Etf.Values[FirstPortfolioToCompare.PortfolioEtfs[0].Etf.Values.Count - 1].Date;
            }
        }
        /// <summary>
        /// Method to handle the ShowEtfDetail Command
        /// </summary>
        /// <param name="portfolioEtf"></param>
        private void ShowEtfDetail(PortfolioEtf portfolioEtf)
        {
            //Send command to the registered messenger user
            Messenger.Default.Send(OperatingCommandsEnum.OpenEtfDetail);
            Messenger.Default.Send(new NotificationMessage<Etf>(portfolioEtf.Etf, OperatingCommandsEnum.ShowEtfDetail.ToString()));
        }
        /// <summary>
        /// Method to handle the EditFirstPortfolioCommand
        /// </summary>
        private void EditFirstPortfolio()
        {
            //Adds available ETFs to portfolio
            foreach (var entry in FirstPortfolioToCompare.PortfolioEtfs)
            {
                entry.AvailableEtfs = ETFs;
            }
            //Send command to the registered messenger user
            Messenger.Default.Send(OperatingCommandsEnum.OpenPortfolioAddEdit);
            Messenger.Default.Send(new NotificationMessage<Portfolio>(FirstPortfolioToCompare, OperatingCommandsEnum.OpenPortfolioEditFirst.ToString()));
        }
        /// <summary>
        /// Method to handle the EditSecondPortfolioCommand
        /// </summary>
        private void EditSecondPortfolio()
        {
            //Adds available ETFs to portfolio
            foreach (var entry in SecondPortfolioToCompare.PortfolioEtfs)
            {
                entry.AvailableEtfs = ETFs;
            }
            //Send command to the registered messenger user
            Messenger.Default.Send(OperatingCommandsEnum.OpenPortfolioAddEdit);
            Messenger.Default.Send(new NotificationMessage<Portfolio>(SecondPortfolioToCompare, OperatingCommandsEnum.OpenPortfolioEditSecond.ToString()));
        }
        /// <summary>
        /// Method to handle the CreatePortfolioCommand
        /// </summary>
        private void CreatePortfolio()
        {
            //Send command to the registered messenger user 
            Messenger.Default.Send(OperatingCommandsEnum.OpenPortfolioAddEdit);
            Messenger.Default.Send(new NotificationMessage<ObservableCollection<Etf>>(ETFs, OperatingCommandsEnum.OpenPortfolioAddEdit.ToString()));
        }
        /// <summary>
        /// Method to handle the NotificationMessagesReceived from Add or Edit Portfolio ViewModel
        /// </summary>
        /// <param name="notification">Notification message</param>
        /// <param name="content">Portfolio that was added/edited</param>
        private void NotificationMessageReceived(string notification, Portfolio content)
        {
            switch (notification)
            {
                //If portfolio was added
                case "0": AllPortfolios.Add(content); break;
                //If FirstPortfolio changed
                case "1": FirstPortfolioToCompare = content; break;
                //If SecondPortfolio changed
                case "2": SecondPortfolioToCompare = content; break;
                default: break;
            }
        }
        /// <summary>
        /// Method to handle the initial ValueGrowth of the portfolios
        /// </summary>
        /// <param name="portfolio"></param>
        private void InitValueGrowth(SeriesCollection portfolio)
        {
            ValueGrowthFirst = new ObservableCollection<ValueGrowth>();
            ValueGrowthSecond = new ObservableCollection<ValueGrowth>();
            for (int j = 0; j < portfolio.Count; j++)
            {
                if (portfolio[j].Values.Count > 0)
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
                                var calcPerf = endPerformance/ entry.Value *100 -100;
                                if (j <= 0)
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
        }
        /// <summary>
        /// Method to handle the initialization of the portfolios
        /// </summary>
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
                MaxDate = ETFs[0].Values[ETFs[0].Values.Count - 1].Date,
                PercentageOfPortfolio = 0.3
            });
            Portfolio7030.PortfolioEtfs.Add(new PortfolioEtf()
            {
                Etf = ETFs[4],
                MinDate = ETFs[4].Values[0].Date,
                MaxDate = ETFs[4].Values[ETFs[4].Values.Count - 1].Date,
                PercentageOfPortfolio = 0.7
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
                Etf = ETFs[8],
                MinDate = ETFs[8].Values[0].Date,
                AvailableEtfs = ETFs,
                MaxDate = ETFs[8].Values[ETFs[8].Values.Count - 1].Date,
                PercentageOfPortfolio = 1
            });
            SecondPortfolioToCompare = PortfolioGlobal;
            AllPortfolios.Add(PortfolioGlobal);
            AllPortfolios.Add(Portfolio7030);
            RaisePropertyChanged("SecondPortfolioToCompare");
        }
        /// <summary>
        /// Method to handle the read in of the given csv datas
        /// </summary>
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
        /// <summary>
        /// Method to calculate the series for first and second portfolio
        /// </summary>
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
                            etfValues[index].Value += (value.Value /firstVal) * etf.PercentageOfPortfolio * 100;
                        }
                        else
                        {
                            if (firstVal != 0)
                            {
                                etfValues.Add(new EtfValue()
                                {
                                    Date = value.Date,
                                    Value = (value.Value /firstVal) * etf.PercentageOfPortfolio *100
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
                            etfValues[index].Value += (value.Value / firstVal) * etf.PercentageOfPortfolio *100 ;
                        }
                        else
                        {
                            if (firstVal != 0)
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
            InitValueGrowth(FirstPortfolioToDisplay);
            RaisePropertyChanged("FirstPortfolioToDisplay");
            RaisePropertyChanged("ValueGrowthFirst");
            RaisePropertyChanged("ValueGrowthSecond");
        }
    }
}
