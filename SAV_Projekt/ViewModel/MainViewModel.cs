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
using System.Windows.Input;
using System.Windows.Media;

namespace SAV_Projekt.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private const string targetDirectory = @"..\..\..\ETF_Data\";
        public ObservableCollection<Portfolio> AllPortfolios { get; set; }
        public Portfolio FirstPortfolioToCompare { get; set; }
        public Portfolio SecondPortfolioToCompare { get; set; }
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
                PercentageOfPortfolio = 0.7m
            });
            Portfolio7030.PortfolioEtfs.Add(new PortfolioEtf()
            {
                Etf = ETFs[1],
                PercentageOfPortfolio = 0.3m
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
                PercentageOfPortfolio = 0.4m
            });
            PortfolioGlobal.PortfolioEtfs.Add(new PortfolioEtf()
            {
                Etf = ETFs[2],
                PercentageOfPortfolio = 0.60m
            });
            SecondPortfolioToCompare = PortfolioGlobal;
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
    }
}
